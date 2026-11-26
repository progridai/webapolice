"""Gera a edição HTML offline a partir dos capítulos, sem dependências externas."""

from html import escape
from html.parser import HTMLParser
from pathlib import Path
import re
import unicodedata


ROOT = Path(__file__).resolve().parent
CHAPTERS = [
    ("README.md", "inicio", "Comece aqui"),
    ("01-primeiros-passos.md", "c01", "01 · Primeiros passos"),
    ("02-conceitos.md", "c02", "02 · Conceitos e glossário"),
    ("03-cadastros.md", "c03", "03 · Cadastros"),
    ("04-apolices.md", "c04", "04 · Apólices e participações"),
    ("05-propostas.md", "c05", "05 · Propostas"),
    ("06-financeiro.md", "c06", "06 · Parcelas e financeiro"),
    ("07-seguranca.md", "c07", "07 · Segurança"),
    ("08-duvidas.md", "c08", "08 · Dúvidas e problemas"),
    ("09-exemplo-pratico.md", "c09", "09 · Exemplo completo"),
]
DESTINATIONS = {filename: chapter for filename, chapter, _ in CHAPTERS}


def slug(value):
    value = unicodedata.normalize("NFKD", value)
    value = "".join(c for c in value if not unicodedata.combining(c))
    value = re.sub(r"[^\w\s-]", "", value.lower())
    return re.sub(r"[\s_]+", "-", value).strip("-")


def target(href, chapter):
    filename, _, anchor = href.partition("#")
    if filename in DESTINATIONS:
        base = DESTINATIONS[filename]
        return "#" + base + ("-" + anchor if anchor else "")
    if not filename:
        return "#" + chapter + ("-" + anchor if anchor else "")
    if filename == "index.html":
        return "#inicio"
    return href


def inline(value, chapter):
    pattern = r"(\[[^\]]+\]\([^)]+\)|\*\*[^*]+\*\*|`[^`]+`)"
    result = []
    for piece in re.split(pattern, value):
        if piece.startswith("["):
            match = re.fullmatch(r"\[([^\]]+)\]\(([^)]+)\)", piece)
            if match:
                label, href = match.groups()
                result.append(
                    '<a href="' + escape(target(href, chapter), quote=True)
                    + '">' + escape(label) + "</a>"
                )
                continue
        if piece.startswith("**") and piece.endswith("**"):
            result.append("<strong>" + inline(piece[2:-2], chapter) + "</strong>")
        elif piece.startswith("`") and piece.endswith("`"):
            result.append("<code>" + escape(piece[1:-1]) + "</code>")
        else:
            result.append(escape(piece))
    return "".join(result)


def render(filename, chapter, label):
    lines = (ROOT / filename).read_text(encoding="utf-8").splitlines()
    headings = [line[3:] for line in lines if line.startswith("## ")]
    toc = '<nav class="page-toc" aria-label="Neste capítulo"><strong>Neste capítulo</strong><ul>'
    toc += "".join(
        '<li><a href="#' + chapter + "-" + slug(h) + '">' + escape(h) + "</a></li>"
        for h in headings
    ) + "</ul></nav>"
    output = ['<article class="chapter" aria-labelledby="' + chapter + '">']
    section_open = False
    i = 0
    while i < len(lines):
        line = lines[i].strip()
        if not line:
            i += 1
            continue
        if line.startswith("# "):
            output.append('<header class="chapter-header"><p class="eyebrow">'
                          + escape(label) + '</p><h1 id="' + chapter + '">'
                          + escape(line[2:]) + "</h1></header>" + toc)
            i += 1
            continue
        if line.startswith("## "):
            if section_open:
                output.append("</section>")
            title = line[3:]
            output.append('<section class="manual-section" data-search="true"><h2 id="'
                          + chapter + "-" + slug(title) + '">'
                          + escape(title) + "</h2>")
            section_open = True
            i += 1
            continue
        picture = re.fullmatch(r"!\[([^\]]+)\]\(([^)]+)\)", line)
        if picture:
            caption, source = picture.groups()
            if not (ROOT / source).is_file():
                raise ValueError("Imagem inexistente: " + source)
            output.append('<figure><img loading="lazy" src="' + escape(source, quote=True)
                          + '" alt="' + escape(caption, quote=True)
                          + '"><figcaption>' + escape(caption) + '</figcaption></figure>')
            i += 1
            continue
        if line.startswith("|"):
            rows = []
            while i < len(lines) and lines[i].strip().startswith("|"):
                cells = [cell.strip() for cell in lines[i].strip().strip("|").split("|")]
                if not all(re.fullmatch(r":?-+:?", cell.replace(" ", "")) for cell in cells):
                    rows.append(cells)
                i += 1
            output.append('<div class="table-wrap" role="region" aria-label="Tabela de referência" tabindex="0"><table><thead><tr>')
            output.extend('<th scope="col">' + inline(cell, chapter) + "</th>" for cell in rows[0])
            output.append("</tr></thead><tbody>")
            for row in rows[1:]:
                output.append("<tr>" + "".join("<td>" + inline(cell, chapter) + "</td>" for cell in row) + "</tr>")
            output.append("</tbody></table></div>")
            continue
        match = re.match(r"^(\d+\. |\- )", line)
        if match:
            ordered = line[0].isdigit()
            tag = "ol" if ordered else "ul"
            output.append("<" + tag + ">")
            pattern = r"^\d+\. " if ordered else r"^\- "
            while i < len(lines) and re.match(pattern, lines[i].strip()):
                item = re.sub(pattern, "", lines[i].strip(), count=1)
                output.append("<li>" + inline(item, chapter) + "</li>")
                i += 1
            output.append("</" + tag + ">")
            continue
        paragraph = [line]
        i += 1
        while i < len(lines) and lines[i].strip() and not re.match(r"^(#|\||\d+\. |\- )", lines[i].strip()):
            paragraph.append(lines[i].strip())
            i += 1
        text = " ".join(paragraph)
        callout = text.startswith(("**Resultado esperado:", "**Ponto de conferência:", "**Atenção:", "**Limite:"))
        output.append('<p' + (' class="callout"' if callout else "") + ">" + inline(text, chapter) + "</p>")
    if section_open:
        output.append("</section>")
    position = [c[0] for c in CHAPTERS].index(filename)
    links = []
    for offset, prefix in [(-1, "← Anterior: "), (1, "Próximo: ")]:
        other = position + offset
        if 0 <= other < len(CHAPTERS):
            _, other_id, other_label = CHAPTERS[other]
            links.append('<a href="#' + other_id + '">' + escape(prefix + other_label) + "</a>")
    output.append('<nav class="chapter-pager" aria-label="Continuar leitura">' + "".join(links) + "</nav></article>")
    return "\n".join(output)


STYLE = """
:root { color-scheme: light; --brand:#D4AF37; --ink:#242424; --muted:#595959; --line:#dedbd2; --paper:#fff; --canvas:#f7f6f2; --soft:#fbf6e5; --focus:#705800; }
* { box-sizing:border-box; }
html { scroll-padding-top:6rem; }
body { margin:0; color:var(--ink); background:var(--canvas); font:17px/1.7 system-ui,-apple-system,"Segoe UI",sans-serif; }
a { color:var(--focus); text-underline-offset:.2em; }
a:hover { text-decoration-thickness:2px; }
:focus-visible { outline:3px solid var(--focus); outline-offset:4px; }
button,input { font:inherit; }
button { border:1px solid var(--line); border-radius:8px; background:var(--paper); color:var(--ink); padding:.5rem .85rem; cursor:pointer; }
button:hover { border-color:var(--focus); }
.skip { position:fixed; top:-6rem; left:1rem; z-index:10; padding:.7rem; background:var(--paper); }
.skip:focus { top:.5rem; }
.topbar { position:sticky; top:0; z-index:4; display:flex; align-items:center; justify-content:space-between; gap:1rem; padding:.7rem 1.5rem; background:var(--paper); border-bottom:3px solid var(--brand); }
.brand { font-weight:750; color:var(--ink); text-decoration:none; line-height:1.35; }
.brand small { display:block; font-size:.75rem; color:var(--muted); font-weight:400; }
.tools { display:flex; gap:.5rem; align-items:center; }
.menu-button { display:none; }
.layout { display:grid; grid-template-columns:280px minmax(0,1fr); max-width:1540px; margin:auto; }
.sidebar { position:sticky; top:5.4rem; height:calc(100vh - 6rem); overflow:auto; padding:1.5rem 1rem; }
.sidebar ol { list-style:none; padding:0; margin:0; }
.sidebar a { display:block; padding:.65rem .8rem; margin:.2rem 0; border-radius:6px; color:var(--ink); text-decoration:none; line-height:1.45; border-left:3px solid transparent; }
.sidebar a:hover,.sidebar a[aria-current] { background:var(--soft); border-left-color:var(--brand); }
.sidebar p { font-size:.8rem; color:var(--muted); padding:.6rem .8rem; }
main { min-width:0; padding:2rem 2.6rem 5rem; }
.search-panel { padding:1.4rem; margin-bottom:2rem; border:1px solid var(--line); border-radius:12px; background:var(--paper); }
.search-panel label { display:block; font-weight:700; }
.search-row { display:flex; gap:.6rem; margin-top:.5rem; }
.search-row input { flex:1; min-width:0; padding:.7rem; border:1px solid var(--muted); border-radius:6px; background:var(--paper); color:var(--ink); }
.help { color:var(--muted); font-size:.9rem; margin:.5rem 0 0; }
#search-results { padding-left:1.5rem; }
#search-results li { margin:.8rem 0; }
#search-results small { display:block; color:var(--muted); }
.chapter { background:var(--paper); border:1px solid var(--line); border-radius:12px; padding:2rem 2.6rem; margin-bottom:2.5rem; }
.eyebrow { color:var(--muted); font-size:.8rem; text-transform:uppercase; letter-spacing:.1em; margin:0; }
h1,h2 { line-height:1.3; text-wrap:balance; }
h1 { font-size:clamp(1.8rem,3vw,2.7rem); margin:.6rem 0 1.5rem; }
h2 { font-size:1.4rem; margin:2.4rem 0 1rem; padding-top:1.1rem; border-top:1px solid var(--line); }
p { margin:.9rem 0; }
li { padding-left:.2rem; margin:.55rem 0; }
.page-toc { background:var(--canvas); border-radius:8px; padding:1rem 1.3rem; font-size:.93rem; }
.page-toc ul { columns:2; padding-left:1.1rem; margin:.5rem 0 0; }
.page-toc li { break-inside:avoid; margin:.35rem 0; }
.table-wrap { overflow-x:auto; margin:1rem 0; }
table { border-collapse:collapse; width:100%; font-size:.92rem; line-height:1.6; }
th,td { text-align:left; vertical-align:top; border:1px solid var(--line); padding:.75rem; }
th { background:var(--canvas); font-weight:700; }
tr:nth-child(even) td { background:#fcfcfa; }
.callout { background:var(--soft); border-left:4px solid var(--brand); padding:1rem 1.2rem; border-radius:0 6px 6px 0; }
code { background:var(--canvas); border:1px solid var(--line); border-radius:4px; padding:.1rem .25rem; overflow-wrap:anywhere; font-size:.9em; }
figure { margin:1.5rem 0; }
figure img { display:block; max-width:100%; height:auto; border:1px solid var(--line); border-radius:8px; }
figcaption { margin-top:.5rem; color:var(--muted); font-size:.85rem; }
.chapter-pager { display:flex; flex-wrap:wrap; justify-content:space-between; gap:1rem; border-top:1px solid var(--line); padding-top:1.4rem; margin-top:2.5rem; font-size:.9rem; }
.footer { text-align:center; font-size:.85rem; color:var(--muted); }
[hidden] { display:none !important; }
@media(max-width:1000px) { .layout { grid-template-columns:235px minmax(0,1fr); } main { padding:1.5rem; } .chapter { padding:1.6rem; } .page-toc ul { columns:1; } }
@media(max-width:760px) { .layout { display:block; } .topbar { padding:.6rem .8rem; } .menu-button { display:inline-block; } .print-button { font-size:.8rem; } .sidebar { display:none; position:static; height:auto; max-height:65vh; padding:.8rem; border-bottom:1px solid var(--line); } .sidebar.open { display:block; } main { padding:1rem .65rem; } .chapter { padding:1.2rem; } .search-panel { padding:1rem; } h1 { font-size:1.8rem; } .brand small { max-width:140px; } }
@media print { @page { margin:18mm; } body { font-size:10.5pt; background:white; color:black; } .topbar,.sidebar,.search-panel,.skip,.chapter-pager,.footer,.chapter>p:first-of-type { display:none; } .layout { display:block; } main { padding:0; } .chapter { border:0; padding:0; margin:0; break-before:page; } .chapter:first-of-type { break-before:auto; } h1 { font-size:23pt; } h2 { font-size:15pt; break-after:avoid; } tr,.callout { break-inside:avoid; } .table-wrap { overflow:visible; } a { color:black; } .page-toc { background:none; border:1px solid #bbb; } }
"""

SCRIPT = """
(() => {
  const menu = document.querySelector('#chapters');
  const toggle = document.querySelector('#menu-toggle');
  toggle.addEventListener('click', () => {
    const open = menu.classList.toggle('open');
    toggle.setAttribute('aria-expanded', String(open));
  });
  document.querySelector('#print').addEventListener('click', () => window.print());
  const normalize = value => value.normalize('NFD').replace(/[\\u0300-\\u036f]/g, '').toLowerCase();
  const search = document.querySelector('#search');
  const results = document.querySelector('#search-results');
  const status = document.querySelector('#search-status');
  const entries = [...document.querySelectorAll('[data-search]')].map(section => ({
    heading: section.querySelector('h2'),
    chapter: section.closest('article').querySelector('h1').textContent,
    text: section.textContent.replace(/\\s+/g, ' ').trim(),
    normalized: normalize(section.textContent.replace(/\\s+/g, ' '))
  }));
  function updateSearch() {
    results.replaceChildren();
    const words = normalize(search.value.trim()).split(/\\s+/).filter(Boolean);
    if (!words.length) { status.textContent = ''; results.hidden = true; return; }
    const matches = entries.filter(entry => words.every(word => entry.normalized.includes(word)));
    results.hidden = false;
    status.textContent = matches.length
      ? `${matches.length} seções encontradas. ${matches.length > 20 ? 'Exibindo as primeiras 20; refine a busca.' : 'Escolha um resultado para ir ao assunto.'}`
      : 'Nenhum resultado. Tente outras palavras, como matrícula, beneficiários ou baixa.';
    matches.slice(0, 20).forEach(entry => {
      const li = document.createElement('li');
      const link = document.createElement('a');
      link.href = '#' + entry.heading.id;
      link.textContent = entry.chapter + ' → ' + entry.heading.textContent;
      const snippet = document.createElement('small');
      const matchPosition = normalize(entry.text).indexOf(words[0]);
      const start = Math.max(0, matchPosition - 65);
      snippet.textContent = (start ? '…' : '') + entry.text.slice(start, start + 210) + '…';
      li.append(link, snippet);
      results.append(li);
    });
  }
  search.addEventListener('input', updateSearch);
  document.querySelector('#clear-search').addEventListener('click', () => {
    search.value = ''; updateSearch(); search.focus();
  });
  function updateNavigation() {
    const hash = location.hash.slice(1) || 'inicio';
    const chapter = hash.split('-')[0];
    document.querySelectorAll('#chapters a').forEach(link => {
      if (link.hash === '#' + chapter) link.setAttribute('aria-current', 'location');
      else link.removeAttribute('aria-current');
    });
  }
  document.addEventListener('click', event => {
    const link = event.target.closest('a[href^="#"]');
    if (!link) return;
    const target = document.getElementById(link.hash.slice(1));
    if (target && /^H[12]$/.test(target.tagName)) {
      target.setAttribute('tabindex', '-1');
      target.focus({preventScroll: true});
    }
    if (menu.contains(link)) {
      menu.classList.remove('open'); toggle.setAttribute('aria-expanded', 'false');
    }
  });
  window.addEventListener('hashchange', updateNavigation);
  updateNavigation();
})();
"""


class LinkChecker(HTMLParser):
    def __init__(self):
        super().__init__()
        self.ids = set()
        self.hrefs = []

    def handle_starttag(self, tag, attrs):
        values = dict(attrs)
        if "id" in values:
            if values["id"] in self.ids:
                raise ValueError("Identificador duplicado: " + values["id"])
            self.ids.add(values["id"])
        if "href" in values:
            self.hrefs.append(values["href"])


def main():
    nav = "".join('<li><a href="#' + chapter + '">' + escape(label) + "</a></li>"
                  for _, chapter, label in CHAPTERS)
    body = "\n".join(render(*chapter) for chapter in CHAPTERS)
    document = '''<!doctype html>
<html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<meta name="description" content="Manual do usuário do WebApolice: cadastros, Apólices, Vidas, Propostas, parcelas e acessos.">
<title>Manual do Usuário · WebApolice</title><style>''' + STYLE + '''</style></head><body>
<a class="skip" href="#conteudo">Ir para o conteúdo</a>
<header class="topbar"><a class="brand" href="#inicio">WebApolice<small>Manual do usuário · versão 0.1</small></a>
<div class="tools"><button id="menu-toggle" class="menu-button" aria-controls="chapters" aria-expanded="false">Capítulos</button>
<button id="print" class="print-button">Imprimir / salvar PDF</button></div></header>
<div class="layout"><nav class="sidebar" id="chapters" aria-label="Capítulos do manual"><ol>''' + nav + '''</ol>
<p>Edição de 25/11/2026<br>Versão para revisão.<br>Conferência visual em andamento.</p></nav>
<main id="conteudo" tabindex="-1"><section class="search-panel" aria-label="Busca no manual">
<label for="search">O que você precisa fazer?</label><div class="search-row"><input id="search" type="search" placeholder="Ex.: criar proposta, matrícula, baixa integral" autocomplete="off" aria-describedby="search-help search-status"><button id="clear-search">Limpar</button></div>
<p id="search-help" class="help">Busca em todos os capítulos, inclusive sem acentos. Funciona offline.</p>
<p id="search-status" class="help" role="status" aria-live="polite"></p><ol id="search-results" hidden></ol>
<noscript><p>A busca e os botões precisam de JavaScript. Os capítulos e links continuam disponíveis abaixo. Use a busca e a impressão do navegador como alternativa.</p></noscript></section>
''' + body + '''<footer class="footer">WebApolice · Manual do usuário · 25/11/2026 · Primeira edição para revisão</footer></main></div><script>''' + SCRIPT + "</script></body></html>\n"
    checker = LinkChecker()
    checker.feed(document)
    for href in checker.hrefs:
        if href.startswith("#"):
            if href[1:] not in checker.ids:
                raise ValueError("Âncora inexistente: " + href)
        elif not (ROOT / href).exists():
            raise ValueError("Arquivo inexistente: " + href)
    if "\ufffd" in document:
        raise ValueError("Caractere de substituição inválido")
    (ROOT / "index.html").write_text(document, encoding="utf-8", newline="\n")
    print(f"Manual gerado: {len(CHAPTERS)} capítulos, {len(checker.ids)} IDs e {len(checker.hrefs)} links verificados.")


if __name__ == "__main__":
    main()
