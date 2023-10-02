var dataTable;

$(document).ready(function () {
    loadTable();
});

function loadTable() {
    dataTable = $('#productTable').DataTable({
        "processing": true,
        "filter": true,
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "discription", "width": "25%" },
            { "data": "price", "width": "15%" },
            { "data": "category.categoryName", "width": "15%" },
            {
                "data": "imgUrl",
                "render": function (data) {
                    return `
                     <img src="${data}" style="width:50px;height:50px" /> 
                    `
                },
                "width": "15%"
            },
            {
                "data": "productId",
                "render": function (data) {
                    return `
                     <a href="/Admin/Product/Upsert?id=${data}" class="btn btn-info w-25 mx-2">
                                <i class="bi bi-pencil-square"> </i> 
                            </a>
                        <a onClick=Delete('/Admin/Product/Delete/${data}') class="btn btn-danger w-25 mx-2">
                                <i class="bi bi-trash3"></i>
                            </a>
                    `
                },
                "width": "15%"
            },
        ]
    });

};

function Delete(url) {

    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
};
