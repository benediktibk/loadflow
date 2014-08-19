all: presentation.pdf report.pdf

presentation.pdf: presentation.tex tikz/*.tex pictures/*.pdf
	pdflatex presentation.tex
	pdflatex presentation.tex

report.pdf: report.tex tikz/*.tex pictures/*.pdf
	pdflatex report.tex
	pdflatex report.tex
	
clean::
	-rm -f *.aux
	-rm -f *.toc
	-rm -f *.out
	-rm -f *.snm
	-rm -f *.nav
	-rm -f *.bbl
	-rm -f *.blg
	-rm -f *.log
	-rm -f *.dvi
	-rm -f *-blx.bib
	-rm -f *.run.xml
	-rm -f texput.log
	-rm -f *.pdf

realclean: clean
	-rm -f *.pdf