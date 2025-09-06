$(document).ready(function () {
  // Khởi tạo DataTable
  var table = $('#promoTable').DataTable({
    dom: 'Bfrtip',
    buttons: ['pdf'],
    paging: false,
    searching: false,
    ordering: false,
    info: false
  });

  // Nút xuất PDF
  $('#exportPdf').on('click', function () {
    table.button('.buttons-pdf').trigger();
  });

  // Lọc trạng thái khi bấm tab
  $('.nav-link').on('click', function () {
    $('.nav-link').removeClass('active');
    $(this).addClass('active');
    var status = $(this).data('status');

    $('#promoTable tbody tr').each(function () {
      var text = $(this).find('td:nth-child(5) span').text().trim(); // Cột Trạng thái

      let match = (
        status === 'all' ||
        (status === 'active' && text === 'Đang áp dụng') ||
        (status === 'inactive' && text === 'Chưa áp dụng') ||
        (status === 'disabled' && text === 'Ngừng áp dụng')
      );

      $(this).toggle(match);
    });
  });
});

// Cập nhật thanh chọn hàng loạt
function updateSelected() {
  const checkboxes = document.querySelectorAll('.rowCheckbox');
  const bulkBar = document.getElementById('bulkActionBar');
  let count = 0;
  checkboxes.forEach(cb => { if (cb.checked) count++; });
  if (count > 0) {
    bulkBar.style.display = 'flex';
    document.getElementById('selectedCount').innerText = `Đã chọn ${count} khuyến mại`;
  } else {
    bulkBar.style.display = 'none';
  }
}

// Checkbox chọn tất cả
function toggleAll(source) {
  const checkboxes = document.querySelectorAll('.rowCheckbox');
  checkboxes.forEach(cb => { cb.checked = source.checked; });
  updateSelected();
}

// Sao chép mã khuyến mại
function copyPromoCode() {
  const code = document.getElementById('promoCodeText').innerText;
  navigator.clipboard.writeText(code).then(() => {
    alert('Đã sao chép mã: ' + code);
  });
}
