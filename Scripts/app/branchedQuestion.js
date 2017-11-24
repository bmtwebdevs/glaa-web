
var branchedQuestion = (function ($) {

    return {
        toggleBranch: toggleBranch,
        radioToggleBranch: radioToggleBranch
    }

    function radioToggleBranch(controlSelector, checkedBranchContainerSelector, uncheckedBranchContainerSelector) {

        $(controlSelector).change(function () {

            var control = $(this);                        

            if (control.is(':checked')) {
                $(checkedBranchContainerSelector).show();
                $(uncheckedBranchContainerSelector).hide();

            } else {
                $(uncheckedBranchContainerSelector).show();
                $(checkedBranchContainerSelector).hide();
            }

        });

        $(controlSelector + ':checked').change();

    }

    function toggleBranch(controlSelector, branchContainerSelector, value) {

        $(controlSelector).change(function () {

            var control = $(this);
            var controlType = control.attr('type');
            var selected;

            switch (controlType) {
                case 'checkbox':
                    selected = control.prop('checked');
                    break;
                case 'radio':
                    if (typeof value === "undefined") {
                        selected = control.is(':checked');
                    }
                    if (typeof value === 'number') {
                        selected = parseInt(control.val(), 10) === value;
                    }
                    if (typeof value === 'boolean') {
                        selected = (control.val() === 'true') === value;
                    }
                    if (typeof value === 'string') {
                        selected = control.val() === value;
                    }
                    break;
                default:
            }

            selected ? $(branchContainerSelector).show() : $(branchContainerSelector).hide();

        });

        $(controlSelector + ':checked').change();
    }
}($));

