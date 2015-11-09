    function confirmDelete() {
        $('#deleteConfirmModal').modal('show');
    }

    $(document).on("click", ".modalLink", function () {
         var passedID = $(this).data('id');
         $('#RowId').val(passedID);
    });

    $(function(id) {
        $("#deleteConfirmModal").on('click', "#deleteConfirm", function() {
            var rowId = $('#RowId').val();
            $.post('Upload/Delete?id=' + rowId, function( data ) {
                location.reload();
            });

            $('#deleteConfirmModal').modal('hide');
        });
    });