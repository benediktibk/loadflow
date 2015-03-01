all: ei_logo.pdf tum_logo.pdf hsa_logo.pdf convergence_border.pdf comparison_runtime.pdf comparison_deviation.pdf comparison_accuracy.pdf convergence_border_vorstadtnetz_1.pdf convergence_border_vorstadtnetz_2.pdf convergence_border_vorstadtnetz_3.pdf big_net_combined_relative_power_error.pdf big_net_combined_runtime.pdf big_net_separate_relative_power_error.pdf big_net_separate_runtime.pdf

%.pdf: %.svg
	@inkscape --without-gui --file=$< --export-pdf=$@