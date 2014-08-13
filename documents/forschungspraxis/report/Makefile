TARGET=report
TIKZ=software_architecture
PICTURES=database_schema

all: $(TARGET).pdf

$(TARGET).pdf: $(TARGET).tex tikz/$(TIKZ).tex Pictures/$(PICTURES).pdf
	pdflatex $(TARGET).tex
	pdflatex $(TARGET).tex
	
clean::
	-rm -f $(TARGET).aux
	-rm -f $(TARGET).toc
	-rm -f $(TARGET).out
	-rm -f $(TARGET).snm
	-rm -f $(TARGET).nav
	-rm -f $(TARGET).bbl
	-rm -f $(TARGET).blg
	-rm -f $(TARGET).log
	-rm -f $(TARGET).dvi
	-rm -f $(TARGET)-blx.bib
	-rm -f $(TARGET).run.xml
	-rm -f texput.log

realclean: clean
	-rm -f $(TARGET).pdf