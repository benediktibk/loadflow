all: ei_logo.pdf tum_logo.pdf hsa_logo.pdf convergence_border.pdf comparison_runtime.pdf comparison_deviation.pdf comparison_accuracy.pdf

%.pdf: %.svg
	@inkscape --without-gui --file=$< --export-pdf=$@