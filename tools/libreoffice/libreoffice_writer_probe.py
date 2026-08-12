#!/usr/bin/env python3
import argparse
import hashlib
import json
import os
import sys
import time

import uno
from com.sun.star.beans import PropertyValue


def prop(name, value):
    p = PropertyValue()
    p.Name = name
    p.Value = value
    return p


def file_url(path):
    return uno.systemPathToFileUrl(os.path.abspath(path))


def seq_count(obj):
    try:
        return int(obj.getCount())
    except Exception:
        try:
            return len(obj.getElementNames())
        except Exception:
            return None


def named_count(doc, method):
    try:
        return seq_count(getattr(doc, method)()) or 0
    except Exception:
        return 0


def enum_count(obj):
    try:
        en = obj.createEnumeration()
        n = 0
        while en.hasMoreElements():
            en.nextElement()
            n += 1
        return n
    except Exception:
        return 0


def iter_enum(obj):
    try:
        en = obj.createEnumeration()
        while en.hasMoreElements():
            yield en.nextElement()
    except Exception:
        return


def service(obj, name):
    try:
        return bool(obj.supportsService(name))
    except Exception:
        return False


def safe_prop(obj, name, default=None):
    try:
        return getattr(obj, name)
    except Exception:
        try:
            return obj.getPropertyValue(name)
        except Exception:
            return default


def sha256(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def connect(pipe_name, attempts=100, delay=0.1):
    local = uno.getComponentContext()
    resolver = local.ServiceManager.createInstanceWithContext("com.sun.star.bridge.UnoUrlResolver", local)
    url = f"uno:pipe,name={pipe_name};urp;StarOffice.ComponentContext"
    last = None
    for _ in range(attempts):
        try:
            ctx = resolver.resolve(url)
            smgr = ctx.ServiceManager
            desktop = smgr.createInstanceWithContext("com.sun.star.frame.Desktop", ctx)
            return ctx, smgr, desktop
        except Exception as ex:
            last = ex
            time.sleep(delay)
    raise RuntimeError(f"uno_connect_failed:{last}")


def observe_text(doc):
    out = {
        "paragraphs": 0,
        "headings": 0,
        "text_tables": named_count(doc, "getTextTables"),
        "bookmarks": named_count(doc, "getBookmarks"),
        "reference_marks": named_count(doc, "getReferenceMarks"),
        "footnotes": named_count(doc, "getFootnotes"),
        "endnotes": named_count(doc, "getEndnotes"),
        "text_frames": named_count(doc, "getTextFrames"),
        "graphic_objects": named_count(doc, "getGraphicObjects"),
        "document_indexes": named_count(doc, "getDocumentIndexes"),
        "redlines": 0,
        "annotations": 0,
        "content_controls": 0,
        "fields": 0,
        "main_text_chars": 0,
        "subflow_text_chars": 0,
        "style_families": [],
        "redline_ids": [],
        "bookmark_names": [],
        "reference_mark_names": [],
    }
    try:
        text = doc.getText()
        out["main_text_chars"] = len(text.getString())
        for para in iter_enum(text):
            if service(para, "com.sun.star.text.Paragraph"):
                out["paragraphs"] += 1
                if safe_prop(para, "ParaStyleName", "") and str(safe_prop(para, "ParaStyleName", "")).lower().startswith("heading"):
                    out["headings"] += 1
            try:
                for portion in iter_enum(para):
                    kind = safe_prop(portion, "TextPortionType", "")
                    if kind == "ContentControl":
                        out["content_controls"] += 1
            except Exception:
                pass
    except Exception:
        pass
    try:
        b = doc.getBookmarks()
        out["bookmark_names"] = sorted(list(b.getElementNames()))
    except Exception:
        pass
    try:
        r = doc.getReferenceMarks()
        out["reference_mark_names"] = sorted(list(r.getElementNames()))
    except Exception:
        pass
    try:
        red = doc.getRedlines()
        out["redlines"] = seq_count(red) or 0
        for x in iter_enum(red):
            ident = safe_prop(x, "RedlineIdentifier", None)
            if ident is not None:
                out["redline_ids"].append(str(ident))
    except Exception:
        pass
    try:
        fields = doc.getTextFields()
        for f in iter_enum(fields):
            out["fields"] += 1
            if service(f, "com.sun.star.text.TextField.Annotation") or service(f, "com.sun.star.text.textfield.Annotation"):
                out["annotations"] += 1
    except Exception:
        pass
    try:
        fams = doc.getStyleFamilies()
        out["style_families"] = sorted(list(fams.getElementNames()))
    except Exception:
        pass
    for getter in ("getTextFrames", "getFootnotes", "getEndnotes"):
        try:
            collection = getattr(doc, getter)()
            names = []
            try:
                names = list(collection.getElementNames())
            except Exception:
                names = []
            if names:
                for name in names:
                    try:
                        x = collection.getByName(name)
                        t = x.getText() if hasattr(x, "getText") else x
                        out["subflow_text_chars"] += len(t.getString())
                    except Exception:
                        pass
            else:
                for x in iter_enum(collection):
                    try:
                        t = x.getText() if hasattr(x, "getText") else x
                        out["subflow_text_chars"] += len(t.getString())
                    except Exception:
                        pass
        except Exception:
            pass
    try:
        tables = doc.getTextTables()
        for name in tables.getElementNames():
            table = tables.getByName(name)
            for cell_name in table.getCellNames():
                try:
                    out["subflow_text_chars"] += len(table.getCellByName(cell_name).getString())
                except Exception:
                    pass
    except Exception:
        pass
    out["redline_ids"] = sorted(out["redline_ids"])
    return out


def document_profile(doc):
    result = {
        "modified": bool(safe_prop(doc, "Modified", False)),
        "title": "",
        "page_count": None,
        "word_count": None,
        "character_count": None,
    }
    try:
        props = doc.getDocumentProperties()
        result["title"] = props.Title
    except Exception:
        pass
    try:
        stats = doc.getDocumentStatistics()
        for p in stats:
            if p.Name == "PageCount":
                result["page_count"] = int(p.Value)
            elif p.Name == "WordCount":
                result["word_count"] = int(p.Value)
            elif p.Name == "CharacterCount":
                result["character_count"] = int(p.Value)
    except Exception:
        pass
    return result


def load(desktop, input_path, readonly=True):
    try:
        macro_never = uno.getConstantByName("com.sun.star.document.MacroExecMode.NEVER_EXECUTE")
    except Exception:
        macro_never = 0
    try:
        no_update = uno.getConstantByName("com.sun.star.document.UpdateDocMode.NO_UPDATE")
    except Exception:
        no_update = 0
    props = [
        prop("Hidden", True),
        prop("ReadOnly", bool(readonly)),
        prop("MacroExecutionMode", macro_never),
        prop("UpdateDocMode", no_update),
    ]
    doc = desktop.loadComponentFromURL(file_url(input_path), "_blank", 0, tuple(props))
    if doc is None:
        raise RuntimeError("writer_load_failed")
    return doc


def close_doc(doc):
    try:
        doc.close(True)
    except Exception:
        try:
            doc.dispose()
        except Exception:
            pass


def store_odt(doc, path):
    props = (prop("FilterName", "writer8"), prop("Overwrite", True))
    doc.storeToURL(file_url(path), props)


def export_pdf(doc, path, pdfua):
    filter_data = []
    if pdfua:
        filter_data.extend([
            prop("PDFUACompliance", True),
            prop("UseTaggedPDF", True),
            prop("ExportBookmarks", True),
            prop("ExportBookmarksToPDFDestination", True),
        ])
    props = [prop("FilterName", "writer_pdf_Export"), prop("Overwrite", True)]
    if filter_data:
        props.append(prop("FilterData", tuple(filter_data)))
    doc.storeToURL(file_url(path), tuple(props))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--pipe", required=True)
    ap.add_argument("--mode", choices=["profile", "observe", "resave", "render-pdf", "unsaved-probe", "terminate"], required=True)
    ap.add_argument("--input")
    ap.add_argument("--output")
    ap.add_argument("--pdfua", action="store_true")
    args = ap.parse_args()
    ctx, smgr, desktop = connect(args.pipe)
    out = {
        "ok": True,
        "mode": args.mode,
        "pipe": args.pipe,
        "pid": os.getpid(),
        "provider_process_ephemeral": True,
        "macro_execution_mode": "NEVER_EXECUTE",
        "update_doc_mode": "NO_UPDATE",
    }
    if args.mode == "terminate":
        out["terminated"] = bool(desktop.terminate())
        print(json.dumps(out, sort_keys=True))
        return
    if args.mode == "profile":
        try:
            cfg = smgr.createInstanceWithContext("com.sun.star.configuration.ConfigurationProvider", ctx)
            out["configuration_provider"] = bool(cfg)
        except Exception:
            out["configuration_provider"] = False
        print(json.dumps(out, sort_keys=True))
        return
    if not args.input:
        raise RuntimeError("input_required")
    doc = load(desktop, args.input, readonly=args.mode in ("observe", "render-pdf"))
    try:
        out["input"] = os.path.abspath(args.input)
        out["input_sha256"] = sha256(args.input)
        out["document"] = document_profile(doc)
        out["writer_semantics"] = observe_text(doc)
        out["unsaved_state"] = bool(safe_prop(doc, "Modified", False))
        if args.mode == "resave":
            if not args.output:
                raise RuntimeError("output_required")
            store_odt(doc, args.output)
            out["output"] = os.path.abspath(args.output)
            out["output_sha256"] = sha256(args.output)
        elif args.mode == "render-pdf":
            if not args.output:
                raise RuntimeError("output_required")
            export_pdf(doc, args.output, args.pdfua)
            out["output"] = os.path.abspath(args.output)
            out["output_sha256"] = sha256(args.output)
            out["pdfua_requested"] = bool(args.pdfua)
        elif args.mode == "unsaved-probe":
            text = doc.getText()
            cursor = text.createTextCursor()
            cursor.gotoEnd(False)
            text.insertString(cursor, " UNSAVED-PROVIDER-PROBE", False)
            out["unsaved_state_after_mutation"] = bool(safe_prop(doc, "Modified", True))
            out["input_sha256_after_unsaved_mutation"] = sha256(args.input)
    finally:
        close_doc(doc)
    print(json.dumps(out, sort_keys=True))


if __name__ == "__main__":
    try:
        main()
    except Exception as ex:
        print(json.dumps({"ok": False, "error": type(ex).__name__, "message": str(ex)}, sort_keys=True))
        sys.exit(2)
