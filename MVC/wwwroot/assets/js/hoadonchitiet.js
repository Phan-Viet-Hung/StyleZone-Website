$(document).ready(function () {
        var table = $('#promoTable').DataTable({
            dom: 'Bfrtip', // Hiển thị nút button
            buttons: [
                {
                    extend: 'pdfHtml5',
                    text: 'Xuất file PDF', // Tùy chỉnh văn bản nút
                    title: 'Invoice Details - INV001', // Tiêu đề file PDF
                    exportOptions: {
                        columns: ':visible' // Chỉ xuất các cột hiển thị
                    }
                }
            ],
            paging: false,
            searching: false,
            ordering: false,
            info: false
        });

        // Khi nhấn vào nút Xuất file thì gọi export PDF
        $('#exportPdf').on('click', function () {
            table.button(0).trigger(); // Kích hoạt nút PDF (vị trí 0 trong mảng buttons)
        });
    });