all: ei_logo.pdf tum_logo.pdf hsa_logo.pdf

%.pdf: %.svg
	@inkscape --without-gui --file=$< --export-pdf=$@