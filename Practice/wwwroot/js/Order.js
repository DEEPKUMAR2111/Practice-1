var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadTable("inprocess");
    }
    else {
        if (url.includes("completed")) {
            loadTable("completed");
        }
        else {
            if (url.includes("pending")) {
                loadTable("pending");
            }
            else {
                if (url.includes("approved")) {
                    loadTable("approved");
                }
                else {
                    loadTable("all");
                }

            }
        }
    }

});

function loadTable(status) {
    dataTable = $('#productTable').DataTable({
        "processing": true,
        "filter": true,
        "ajax": {
            "url": "/Admin/Order/GetAllOrder?status=" + status
        },
        "columns": [
            { "data": "id", "width": "8%" },
            { "data": "name", "width": "12%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "orderStatus", "width": "12%" },
            { "data": "paymentStatus", "width": "12%" },
            { "data": "orderTotal", "width": "12%" },
            { "data": "postalCode", "width": "12%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                     <a  href="/Admin/Order/Details?orderid=${data}" class="btn btn-danger  mx-2 text-center">
                             <i class="bi bi-eye-fill"></i> 
                            </a> `
                },
                "width": "8%"
            },
        ]
    });

};