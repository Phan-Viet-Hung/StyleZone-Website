// $(document).ready(function () {
//   $('#productTable').DataTable({
//     responsive: true,
//     dom: 'Bfrtip',
//     buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
//   });
// });

$(document).ready(function () {
  $('#productTable').DataTable({
    responsive: true,
    dom: 'Bfrtip',
    buttons: ['copy', 'csv', 'excel', 'pdf', 'print'],
    language: {
      search: "" // Bỏ chữ "Search:"
    }
  });
});



