all: ei_logo.pdf tum_logo.pdf

%.pdf: %.svg
	@inkscape --without-gui --file=$< --export-pdf=$@