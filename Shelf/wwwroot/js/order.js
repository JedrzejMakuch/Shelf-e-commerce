
var dataTable;
$(document).ready(function () {
    var params = new URLSearchParams(window.location.search);
    var status = params.get("status");

    if (status) {
        loadDataTable(status); 
    } else {
        loadDataTable(); 
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        ajax: {
            url: '/admin/order/getall?status=' + status
        },
        columns: [
            { data: 'id', width: "25%" },
            { data: 'name', width: "15%" },
            { data: 'phoneNumber', width: "15%" },
            { data: 'applicationUser.email', width: "10%" },
            { data: 'orderStatus', width: "20%" },
            { data: 'orderTotal', width: "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                                <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                            </div>`
            } , "width": "25%" }
        ]
    });
}
