#set document(title: "DOCSeye E12 E13 Early Renderer Qualification", author: "StealthEyeLLC")
#set page(paper: "us-letter", margin: (x: 0.75in, y: 0.7in), numbering: "1", header: context [DOCSeye renderer qualification], footer: context [#counter(page).display()])
#set text(font: ("Segoe UI", "Nirmala UI", "Yu Gothic", "Segoe UI Emoji", "Arial"), size: 10pt, lang: "en")
#set par(justify: true, leading: 0.65em)

= Native Semantic Render Qualification <obj_h1>
#par[This document stresses semantic structure, multilingual shaping, page reflow, links, figures, tables, notes, and exact source locations. #link("https://example.com")[External link].] <obj_p1>
#for i in range(0, 18) [
  #par[Early expansion #(i + 1): This edit intentionally adds substantial semantic text before every later pressure object so the derived layout must reflow while later semantic labels remain the same.]
]

== Script coverage <obj_h2>
#par[Latin OpenType text with ligatures: office affinity efficient final.] <obj_latin>
#par[#text(lang: "ar", dir: rtl)[#("\u{0645}\u{0631}\u{062D}\u{0628}\u{0627} \u{0628}\u{0627}\u{0644}\u{0639}\u{0627}\u{0644}\u{0645} \u{0645}\u{0633}\u{062A}\u{0646}\u{062F} \u{0627}\u{062E}\u{062A}\u{0628}\u{0627}\u{0631}")]] <obj_arabic>
#par[Mixed bidi: ABC #text(lang: "ar", dir: rtl)[#("\u{0645}\u{0631}\u{062D}\u{0628}\u{0627}")] 123 XYZ.] <obj_bidi>
#par[#text(lang: "hi")[#("\u{0928}\u{092E}\u{0938}\u{094D}\u{0924}\u{0947} \u{0926}\u{0941}\u{0928}\u{093F}\u{092F}\u{093E}")]] <obj_devanagari>
#par[#text(lang: "ja")[#("\u{65E5}\u{672C}\u{8A9E}\u{306E}\u{30EC}\u{30A4}\u{30A2}\u{30A6}\u{30C8}\u{691C}\u{8A3C}")]] <obj_cjk>
#par[Combining sequence exact: e\u{0301} A\u{030A} n\u{0303}. Emoji ZWJ: #("\u{1F469}\u{200D}\u{1F469}\u{200D}\u{1F467}\u{200D}\u{1F467}") and #("\u{1F468}\u{200D}\u{1F4BB}").] <obj_combining>

== Accessible authored table <obj_h3>
#figure(
  table(
    columns: (1.2fr, 1fr, 1fr),
    inset: 5pt,
    stroke: 0.6pt,
    table.header(repeat: true,
      [Language], [Sample], [Status],
    ),
    [Latin], [office], [required],
    [Arabic], [#text(lang: "ar", dir: rtl)[#("\u{0645}\u{0631}\u{062D}\u{0628}\u{0627}")]], [required],
    [Devanagari], [#text(lang: "hi")[#("\u{0928}\u{092E}\u{0938}\u{094D}\u{0924}\u{0947}")]], [required],
    [Japanese], [#text(lang: "ja")[#("\u{65E5}\u{672C}\u{8A9E}")]], [required],
  ),
  caption: [Script qualification matrix],
) <obj_table>

== Figure and note pressure <obj_h4>
#figure(image("semantic-figure.svg", width: 55%, alt: "A circle beside an outlined triangle inside a rounded rectangle."), caption: [Inert SVG semantic figure]) <obj_figure>
#par[This sentence owns a note#footnote[Footnote content must remain in logical reading order when the page changes.] and enough following content to force pagination.] <obj_note_par>

#pagebreak()
#set page(columns: 2)
== Two-column pressure <obj_h5>
#for i in range(0, 22) [
  #par[Pressure paragraph #(i + 1): A local edit near the beginning should change later line and page breaks without changing semantic identity. Hyphenation-sensitive words include internationalization, representation, interoperability, correspondence, deterministic, accessibility, and extraordinary.]
]

#colbreak()
=== Explicit column continuation <obj_h6>
#for i in range(0, 14) [
  #par[Continuation #(i + 1): Structured text after an explicit column break keeps semantic order while physical regions change.]
]