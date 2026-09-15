/**
 * HRM Modern DatePicker with "Xác nhận" confirmation button
 * Specifically designed for Nhị Gia HRM system
 */
(function ($) {
    'use strict';

    var MONTH_NAMES = [
        'Tháng 01', 'Tháng 02', 'Tháng 03', 'Tháng 04',
        'Tháng 05', 'Tháng 06', 'Tháng 07', 'Tháng 08',
        'Tháng 09', 'Tháng 10', 'Tháng 11', 'Tháng 12'
    ];
    var DAY_NAMES = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

    var $overlay = null;
    var currentInput = null;
    var viewYear = 2026;
    var viewMonth = 1; // 1-12
    var selectedDate = null; // { year, month, day }

    function getSelectedOrCurrentDate(val) {
        if (val) {
            var parts = val.split('-');
            if (parts.length === 3) {
                var y = parseInt(parts[0], 10);
                var m = parseInt(parts[1], 10);
                var d = parseInt(parts[2], 10);
                if (!isNaN(y) && !isNaN(m) && !isNaN(d)) {
                    return { year: y, month: m, day: d };
                }
            }
            var dparts = val.split('/');
            if (dparts.length === 3) {
                var d2 = parseInt(dparts[0], 10);
                var m2 = parseInt(dparts[1], 10);
                var y2 = parseInt(dparts[2], 10);
                if (!isNaN(y2) && !isNaN(m2) && !isNaN(d2)) {
                    return { year: y2, month: m2, day: d2 };
                }
            }
        }
        var now = new Date();
        return { year: now.getFullYear(), month: now.getMonth() + 1, day: now.getDate() };
    }

    function createOverlay() {
        if ($overlay) return;

        var html = [
            '<div id="hrmDatepickerOverlay" class="hrm-datepicker-overlay" style="display:none;">',
            '  <div class="hrm-dp-header">',
            '    <button type="button" class="hrm-dp-nav-btn" id="hrmDpPrev"><i class="material-icons" style="font-size:16px;">chevron_left</i></button>',
            '    <div class="hrm-dp-title">',
            '      <select id="hrmDpMonth" class="hrm-dp-select"></select>',
            '      <select id="hrmDpYear" class="hrm-dp-select"></select>',
            '    </div>',
            '    <button type="button" class="hrm-dp-nav-btn" id="hrmDpNext"><i class="material-icons" style="font-size:16px;">chevron_right</i></button>',
            '  </div>',
            '  <div class="hrm-dp-weekdays"></div>',
            '  <div class="hrm-dp-days" id="hrmDpDaysGrid"></div>',
            '  <div class="hrm-dp-footer">',
            '    <button type="button" class="hrm-dp-btn-clear" id="hrmDpBtnClear">Xóa</button>',
            '    <button type="button" class="hrm-dp-btn-confirm" id="hrmDpBtnConfirm">Xác nhận</button>',
            '  </div>',
            '</div>'
        ].join('');

        $('body').append(html);
        $overlay = $('#hrmDatepickerOverlay');

        // Populate Month select
        var $mSelect = $('#hrmDpMonth');
        for (var i = 0; i < 12; i++) {
            $mSelect.append('<option value="' + (i + 1) + '">' + MONTH_NAMES[i] + '</option>');
        }

        // Populate Year select
        var $ySelect = $('#hrmDpYear');
        for (var y = 2020; y <= 2035; y++) {
            $ySelect.append('<option value="' + y + '">Năm ' + y + '</option>');
        }

        // Populate weekdays header
        var $wd = $overlay.find('.hrm-dp-weekdays');
        for (var w = 0; w < DAY_NAMES.length; w++) {
            $wd.append('<span>' + DAY_NAMES[w] + '</span>');
        }

        // Event: Nav Previous Month
        $('#hrmDpPrev').on('click', function (e) {
            e.stopPropagation();
            if (viewMonth === 1) {
                viewMonth = 12;
                viewYear--;
            } else {
                viewMonth--;
            }
            renderGrid();
        });

        // Event: Nav Next Month
        $('#hrmDpNext').on('click', function (e) {
            e.stopPropagation();
            if (viewMonth === 12) {
                viewMonth = 1;
                viewYear++;
            } else {
                viewMonth++;
            }
            renderGrid();
        });

        // Event: Select Month / Year dropdowns
        $('#hrmDpMonth').on('change', function () {
            viewMonth = parseInt($(this).val(), 10);
            renderGrid();
        });
        $('#hrmDpYear').on('change', function () {
            viewYear = parseInt($(this).val(), 10);
            renderGrid();
        });

        // Event: Day click
        $overlay.on('click', '.hrm-dp-day:not(.empty)', function (e) {
            e.stopPropagation();
            var y = parseInt($(this).data('year'), 10);
            var m = parseInt($(this).data('month'), 10);
            var d = parseInt($(this).data('day'), 10);

            selectedDate = { year: y, month: m, day: d };
            $overlay.find('.hrm-dp-day').removeClass('selected');
            $(this).addClass('selected');
        });

        // Event: Day double-click -> directly confirm
        $overlay.on('dblclick', '.hrm-dp-day:not(.empty)', function (e) {
            e.stopPropagation();
            var y = parseInt($(this).data('year'), 10);
            var m = parseInt($(this).data('month'), 10);
            var d = parseInt($(this).data('day'), 10);
            selectedDate = { year: y, month: m, day: d };
            confirmSelection();
        });

        // Event: Clear button
        $('#hrmDpBtnClear').on('click', function (e) {
            e.stopPropagation();
            if (currentInput) {
                $(currentInput).val('').trigger('change').trigger('input');
            }
            hideOverlay();
        });

        // Event: Confirm button ("XÁC NHẬN")
        $('#hrmDpBtnConfirm').on('click', function (e) {
            e.stopPropagation();
            confirmSelection();
        });

        // Prevent overlay click from closing
        $overlay.on('click', function (e) {
            e.stopPropagation();
        });

        // Close when clicking outside
        $(document).on('click', function (e) {
            if ($overlay.is(':visible') && !$(e.target).closest('#hrmDatepickerOverlay, [data-hrm-datepicker]').length) {
                hideOverlay();
            }
        });

        // Close on ESC
        $(document).on('keydown', function (e) {
            if (e.key === 'Escape' && $overlay.is(':visible')) {
                hideOverlay();
            }
        });
    }

    function confirmSelection() {
        if (currentInput && selectedDate) {
            var mm = (selectedDate.month < 10 ? '0' : '') + selectedDate.month;
            var dd = (selectedDate.day < 10 ? '0' : '') + selectedDate.day;
            var formatted = selectedDate.year + '-' + mm + '-' + dd;
            $(currentInput).val(formatted).trigger('change').trigger('input');
        }
        hideOverlay();
    }

    function renderGrid() {
        $('#hrmDpMonth').val(viewMonth);
        $('#hrmDpYear').val(viewYear);

        var $grid = $('#hrmDpDaysGrid');
        $grid.empty();

        var firstDayIndex = new Date(viewYear, viewMonth - 1, 1).getDay(); // 0 = Sun, 1 = Mon ...
        var startOffset = (firstDayIndex === 0 ? 6 : firstDayIndex - 1); // Monday is 0

        var daysInPrevMonth = new Date(viewYear, viewMonth - 1, 0).getDate();
        var daysInCurrentMonth = new Date(viewYear, viewMonth, 0).getDate();

        // Trailing days of previous month
        for (var p = startOffset - 1; p >= 0; p--) {
            var prevD = daysInPrevMonth - p;
            var prevM = viewMonth === 1 ? 12 : viewMonth - 1;
            var prevY = viewMonth === 1 ? viewYear - 1 : viewYear;
            $grid.append('<div class="hrm-dp-day other-month" data-year="' + prevY + '" data-month="' + prevM + '" data-day="' + prevD + '">' + prevD + '</div>');
        }

        // Current month days
        var now = new Date();
        var todayYear = now.getFullYear();
        var todayMonth = now.getMonth() + 1;
        var todayDay = now.getDate();

        for (var d = 1; d <= daysInCurrentMonth; d++) {
            var isSelected = (selectedDate && selectedDate.year === viewYear && selectedDate.month === viewMonth && selectedDate.day === d);
            var isToday = (viewYear === todayYear && viewMonth === todayMonth && d === todayDay);

            var cls = 'hrm-dp-day';
            if (isSelected) cls += ' selected';
            if (isToday) cls += ' today';

            $grid.append('<div class="' + cls + '" data-year="' + viewYear + '" data-month="' + viewMonth + '" data-day="' + d + '">' + (d < 10 ? '0' + d : d) + '</div>');
        }

        // Leading days of next month
        var totalCells = startOffset + daysInCurrentMonth;
        var remaining = (7 - (totalCells % 7)) % 7;
        if (totalCells + remaining < 35) {
            remaining += 7;
        }
        for (var n = 1; n <= remaining; n++) {
            var nextM = viewMonth === 12 ? 1 : viewMonth + 1;
            var nextY = viewMonth === 12 ? viewYear + 1 : viewYear;
            $grid.append('<div class="hrm-dp-day other-month" data-year="' + nextY + '" data-month="' + nextM + '" data-day="' + n + '">' + (n < 10 ? '0' + n : n) + '</div>');
        }
    }

    function showOverlay(input) {
        createOverlay();
        currentInput = input;

        var val = $(input).val();
        selectedDate = getSelectedOrCurrentDate(val);
        viewYear = selectedDate.year;
        viewMonth = selectedDate.month;

        renderGrid();

        var rect = input.getBoundingClientRect();
        var scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        var scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

        var top = rect.bottom + scrollTop + 4;
        var left = rect.left + scrollLeft;

        // Ensure within screen width
        var overlayWidth = 300;
        if (left + overlayWidth > $(window).width() - 16) {
            left = $(window).width() - overlayWidth - 16;
        }
        if (left < 10) left = 10;

        // If off bottom of screen, show above
        var overlayHeight = 320;
        if (rect.bottom + overlayHeight > $(window).height() && rect.top > overlayHeight) {
            top = rect.top + scrollTop - overlayHeight - 4;
        }

        $overlay.css({
            top: top + 'px',
            left: left + 'px',
            zIndex: 99999
        }).fadeIn(120);
    }

    function hideOverlay() {
        if ($overlay) {
            $overlay.fadeOut(100);
        }
        currentInput = null;
    }

    // Export function to initialize datepicker elements
    window.initHrmDatepickers = function () {
        $('input[type="date"]').each(function () {
            var $el = $(this);
            $el.attr('type', 'text')
               .attr('data-hrm-datepicker', 'true')
               .attr('autocomplete', 'off')
               .addClass('hrm-date-input');
        });
    };

    // Auto setup on ready & dynamic events
    $(function () {
        window.initHrmDatepickers();

        // When any bootstrap modal is shown, re-init in modal
        $(document).on('shown.bs.modal', function () {
            window.initHrmDatepickers();
        });

        // Bind clicks on any input with data-hrm-datepicker or class hrm-date-input
        $(document).on('click focus', '[data-hrm-datepicker], .hrm-date-input', function (e) {
            e.preventDefault();
            showOverlay(this);
        });
    });

    // Fix: Xử lý an toàn mở modal duyệt bảng lương và phát hành phiếu lương (tránh xung đột data-toggle và onclick)
    document.addEventListener('click', function (e) {
        var btn = e.target && e.target.closest ? e.target.closest('.btn-payroll-publish, .btn-payroll-approve, .btn-payroll-reject, .btn-payroll-finalize, [data-target="#modalBatchPublish"], [data-target="#modalBatchApprove"], [data-target="#modalBatchReject"], [data-target="#modalBatchFinalize"], [data-target="#modalAddAllowance"], [data-target="#modalAddDeduction"], [data-target="#modalAddAdvance"]') : null;
        if (!btn) return;

        e.preventDefault();
        e.stopPropagation();

        var targetSelector = btn.getAttribute('data-target') || btn.getAttribute('data-bs-target');
        if (!targetSelector) {
            if (btn.classList.contains('btn-payroll-publish')) targetSelector = '#modalBatchPublish';
            else if (btn.classList.contains('btn-payroll-approve')) targetSelector = '#modalBatchApprove';
            else if (btn.classList.contains('btn-payroll-reject')) targetSelector = '#modalBatchReject';
            else if (btn.classList.contains('btn-payroll-finalize')) targetSelector = '#modalBatchFinalize';
        }

        if (targetSelector) {
            var modalEl = document.querySelector(targetSelector);
            if (modalEl) {
                if (window.jQuery && typeof jQuery.fn.modal === 'function') {
                    $(modalEl).modal('show');
                } else if (window.bootstrap && bootstrap.Modal) {
                    if (typeof bootstrap.Modal.getOrCreateInstance === 'function') {
                        bootstrap.Modal.getOrCreateInstance(modalEl).show();
                    } else {
                        new bootstrap.Modal(modalEl).show();
                    }
                } else {
                    modalEl.style.display = 'block';
                    modalEl.classList.add('in');
                    modalEl.setAttribute('aria-hidden', 'false');
                    var bd = document.createElement('div');
                    bd.className = 'modal-backdrop fade in';
                    document.body.appendChild(bd);
                    document.body.classList.add('modal-open');
                }
            }
        }
    }, true);

})(jQuery);


