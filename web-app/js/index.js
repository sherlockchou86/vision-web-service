
$(function()
{
    // initialize the slide control
    var mySwiper = new Swiper('.swiper-container',{
        pagination: '.pagination',
        paginationClickable: true,
        centeredSlides: true,
        slidesPerView: 3,
        watchActiveIndex: true,
        imageClickable: true
    })

    // sync the file input and text input
    $(".upload .upload-input-file").change(function () {        
        if ($(this).parent().html().indexOf("class=\"upload-url\"") != -1) {
            var fileUrl = $(this).val();
            $(this).parent().children(".upload-url").val(fileUrl);
        }
    });

    // bind click to submit online url detect
    $("#submit_url").click(function()
    {
        $.post("/detect", {"image_url": $("#image_url").val()}, function(data, status, xhr){
            alert(data)
        });
    });

    // bind click to submit local image detect
    $("#submit_image").click(function(){

    });
})
