all: ei_logo.pdf tum_logo.pdf hsa_logo.pdf convergence_border.pdf comparison_runtime.pdf comparison_deviation.pdf comparison_accuracy.pdf convergence_border_vorstadtnetz_1.pdf convergence_border_vorstadtnetz_2.pdf convergence_border_vorstadtnetz_3.pdf

%.pdf: %.svg
	@inkscape --without-gui --file=$< --export-pdf=$@