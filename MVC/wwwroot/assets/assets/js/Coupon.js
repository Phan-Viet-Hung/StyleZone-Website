$(document).ready(function () {
    var table = $('#promoTable').DataTable({
      dom: 'Bfrtip',
      buttons: ['pdf'],
      paging: false,
      searching: false,
      ordering: false,
      info: false
    });

    // Khi nhấn vào nút Xuất file thì gọi export PDF
    $('#exportPdf').on('click', function () {
      table.button('.buttons-pdf').trigger();
    });
  });

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

  function toggleAll(source) {
    const checkboxes = document.querySelectorAll('.rowCheckbox');
    checkboxes.forEach(cb => { cb.checked = source.checked; });
    updateSelected();
  }
  function copyPromoCode() {
  const code = document.getElementById('promoCodeText').innerText;
  navigator.clipboard.writeText(code).then(() => {
    alert('Đã sao chép mã: ' + code);
  });

}
$(document).ready(function () {
    // Đổi tab lọc trạng thái
    $('.nav-link').on('click', function () {
      $('.nav-link').removeClass('active');
      $(this).addClass('active');
      var status = $(this).data('status');

      $('#promoTable tbody tr').each(function () {
        var text = $(this).find('td:nth-child(4) span').text().trim(); // Lấy text trạng thái

        let match = (
          status === 'all' ||
          (status === 'active' && text === 'Đang áp dụng') ||
          (status === 'inactive' && text === 'Chưa áp dụng') ||
          (status === 'disabled' && text === 'Ngừng áp dụng')
        );

        $(this).toggle(match);
      });
    });

    // Hàm toggleAll và updateSelected bạn đã có sẵn thì cứ dùng tiếp nhé
  });
