ea.addMethod("CompanyRegistrationDateIsValid",
    function(date) {
        if (!$("#LegalStatus:checked").length || $("#LegalStatus:checked").val() !== 2) {
            return true;
        }
        var valid = true;
        $("input.date-part").each(function(index, elem) {
            valid = $(elem).val() && valid;
        });
        return valid;
    });
$.validator.addMethod("chkSelectAtLeastOne", function () {
    return $('input[type="checkbox"][data-val-required]:checked').length > 0;
}, "Please select at least one option.");
$.validator.addMethod("rbxSelectAtLeastOne", function () {
    return $('input[type="radio"][data-val-required]:checked').length > 0;
}, "Please select one option.");
$.validator.setDefaults({
    rules: {
        checkbox_list: { chkSelectAtLeastOne: true },
        radio_list: { rbxSelectAtLeastOne: true }
    },
    showErrors: function (errorMap, errorList) {
        $(".error-summary-list").empty();
        $(".error-summary").css("display", "none");
        $(".form-group").removeClass("form-group-error");

        if (document.title.indexOf("Error: ") > -1) {
            document.title = document.title.substring(7);
        }

        if (errorList.length) {
            for (var i = 0; i < errorList.length; i++) {
                // inline
                $(errorList[i].element).closest(".form-group").addClass("form-group-error");
                // summary
                var a = document.createElement("a");
                a.href = "#" + $(errorList[i].element).siblings("legend").attr("id");
                a.innerText = errorList[i].message;

                var li = document.createElement("li");
                li.appendChild(a);
                $(".error-summary-list").append(li);
            }

            $(".error-summary").css("display", "block");
            document.title = "Error: " + document.title;
        }
        this.defaultShowErrors();
    },
    onfocusout: function (element) {
        // Only do dynamic validation if the form has already been submitted
        if (Object.keys(this.submitted).length > 0) {
            $(element.form).valid();
        }
    },
    // adapted from https://github.com/jquery-validation/jquery-validation/blob/master/src/core.js
    onkeyup: function(element, event) {
        // Avoid revalidate the field when pressing one of the following keys
        // Shift       => 16
        // Ctrl        => 17
        // Alt         => 18
        // Caps lock   => 20
        // End         => 35
        // Home        => 36
        // Left arrow  => 37
        // Up arrow    => 38
        // Right arrow => 39
        // Down arrow  => 40
        // Insert      => 45
        // Num lock    => 144
        // AltGr key   => 225
        var excludedKeys = [
            16, 17, 18, 20, 35, 36, 37,
            38, 39, 40, 45, 144, 225
        ];

        // 9 is tab, don't alter its behaviour!
        if (event.which === 9 && this.elementValue(element) === "" || $.inArray(event.keyCode, excludedKeys) !== -1) {
            return;
        } else if (Object.keys(this.submitted).length > 0 || element.name in this.invalid) {
            $(element.form).valid();
        }
    }
});
