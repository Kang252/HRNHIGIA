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

    // People read dd/MM/yyyy; scripts and the server keep reading yyyy-MM-dd from the same field.
    var nativeValue = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value');
    var enhanced = new WeakSet();

    function pad(n) { return (n < 10 ? '0' : '') + n; }
    function isoDate(y, m, d) {
        var date = new Date(y, m - 1, d);
        return y >= 1000 && date.getFullYear() === y && date.getMonth() === m - 1 && date.getDate() === d
            ? y + '-' + pad(m) + '-' + pad(d) : '';
    }
    function toIso(text) {
        var value = String(text == null ? '' : text).trim();
        var iso = /^(\d{4})-(\d{1,2})-(\d{1,2})/.exec(value);
        if (iso) return isoDate(+iso[1], +iso[2], +iso[3]);
        var local = /^(\d{1,2})[\/.\-](\d{1,2})[\/.\-](\d{4})$/.exec(value);
        return local ? isoDate(+local[3], +local[2], +local[1]) : '';
    }
    function toDisplay(value) {
        var iso = toIso(value);
        return iso ? iso.split('-').reverse().join('/') : String(value == null ? '' : value);
    }
    function checkDate(el) {
        var text = nativeValue.get.call(el).trim(), iso = toIso(text);
        var min = toIso(el.getAttribute('min')), max = toIso(el.getAttribute('max'));
        el.setCustomValidity(!text ? ''
            : !iso ? 'Nhập ngày theo dạng ngày/tháng/năm, ví dụ 05/12/2026.'
            : min && iso < min ? 'Ngày phải từ ' + toDisplay(min) + ' trở đi.'
            : max && iso > max ? 'Ngày không được sau ' + toDisplay(max) + '.' : '');
    }
    function enhance(el) {
        if (enhanced.has(el)) return;
        enhanced.add(el);
        var current = nativeValue.get.call(el);
        el.type = 'text';
        el.setAttribute('data-hrm-datepicker', 'true');
        el.setAttribute('autocomplete', 'off');
        el.classList.add('hrm-date-input');
        if (!el.getAttribute('placeholder')) el.setAttribute('placeholder', 'dd/mm/yyyy');
        if (window.matchMedia && window.matchMedia('(pointer: coarse)').matches) el.setAttribute('inputmode', 'none');
        Object.defineProperty(el, 'value', {
            configurable: true, enumerable: true,
            get: function () { return toIso(nativeValue.get.call(el)); },
            set: function (value) { nativeValue.set.call(el, toDisplay(value)); checkDate(el); }
        });
        el.value = current;
    }
    function enhanceWithin(node) {
        if (node.nodeType !== 1 && node !== document) return;
        if (node.matches && node.matches('input[type="date"], input[data-hrm-datepicker]')) enhance(node);
        if (node.querySelectorAll) node.querySelectorAll('input[type="date"], input[data-hrm-datepicker]').forEach(enhance);
    }

    // Typed text is tidied up, native form posts send yyyy-MM-dd and a reset shows its default again.
    document.addEventListener('input', function (event) { if (enhanced.has(event.target)) checkDate(event.target); }, true);
    document.addEventListener('change', function (event) {
        if (enhanced.has(event.target) && toIso(nativeValue.get.call(event.target))) event.target.value = event.target.value;
    }, true);
    document.addEventListener('formdata', function (event) {
        var names = {};
        Array.prototype.forEach.call(event.target.elements, function (el) { if (el.name && enhanced.has(el)) names[el.name] = true; });
        Object.keys(names).forEach(function (name) {
            if (!event.formData.has(name)) return;
            event.formData.delete(name);
            Array.prototype.forEach.call(event.target.elements, function (el) {
                if (el.name === name && !el.disabled) event.formData.append(name, el.value);
            });
        });
    });
    document.addEventListener('reset', function (event) {
        var form = event.target;
        setTimeout(function () {
            Array.prototype.forEach.call(form.elements, function (el) { if (enhanced.has(el)) el.value = el.getAttribute('value') || ''; });
        }, 0);
    });

    function setDate(input, iso) {
        input.value = iso;
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change', { bubbles: true }));
    }
    function allowed(iso) {
        if (!currentInput) return true;
        var min = toIso(currentInput.getAttribute('min')), max = toIso(currentInput.getAttribute('max'));
        return (!min || iso >= min) && (!max || iso <= max);
    }

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
        var thisYear = new Date().getFullYear();
        for (var y = thisYear + 10; y >= thisYear - 80; y--) {
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
        $overlay.on('click', '.hrm-dp-day:not(.empty):not(.disabled)', function (e) {
            e.stopPropagation();
            var y = parseInt($(this).data('year'), 10);
            var m = parseInt($(this).data('month'), 10);
            var d = parseInt($(this).data('day'), 10);

            selectedDate = { year: y, month: m, day: d };
            $overlay.find('.hrm-dp-day').removeClass('selected');
            $(this).addClass('selected');
        });

        // Event: Day double-click -> directly confirm
        $overlay.on('dblclick', '.hrm-dp-day:not(.empty):not(.disabled)', function (e) {
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
                setDate(currentInput, '');
            }
            hideOverlay();
        });

        // Event: Confirm button ("XÁC NHẬN")
        $('#hrmDpBtnConfirm').on('click', function (e) {
            e.stopPropagation();
            confirmSelection();
        });

        // Prevent overlay click from closing; an open dialog must not pull focus out of the month and year lists.
        $overlay.on('click focusin', function (e) {
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
            if ((e.key === 'Escape' || e.key === 'Tab' && enhanced.has(e.target)) && $overlay.is(':visible')) {
                hideOverlay();
            }
        });
    }

    function confirmSelection() {
        if (currentInput && selectedDate) {
            setDate(currentInput, isoDate(selectedDate.year, selectedDate.month, selectedDate.day));
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
            $grid.append('<div class="hrm-dp-day other-month' + (allowed(isoDate(prevY, prevM, prevD)) ? '' : ' disabled') + '" data-year="' + prevY + '" data-month="' + prevM + '" data-day="' + prevD + '">' + prevD + '</div>');
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
            if (!allowed(isoDate(viewYear, viewMonth, d))) cls += ' disabled';

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
            $grid.append('<div class="hrm-dp-day other-month' + (allowed(isoDate(nextY, nextM, n)) ? '' : ' disabled') + '" data-year="' + nextY + '" data-month="' + nextM + '" data-day="' + n + '">' + (n < 10 ? '0' + n : n) + '</div>');
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

        $overlay.stop(true, true).css({ display: 'block', visibility: 'hidden' });
        var overlayWidth = $overlay.outerWidth(), overlayHeight = $overlay.outerHeight();
        $overlay.css({ display: 'none', visibility: '' });

        var left = Math.max(10, Math.min(rect.left, window.innerWidth - overlayWidth - 16)) + scrollLeft;

        // Below the field, above it, or wherever the whole calendar (with its Confirm button) fits.
        var below = rect.bottom + 4, above = rect.top - overlayHeight - 4;
        var top = scrollTop + (below + overlayHeight <= window.innerHeight - 8 ? below
            : above >= 8 ? above
            : Math.max(8, window.innerHeight - overlayHeight - 8));

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
    window.initHrmDatepickers = function () { enhanceWithin(document); };

    // Auto setup on ready & dynamic events
    $(function () {
        window.initHrmDatepickers();

        // Forms built later by scripts get the same date fields.
        new MutationObserver(function (changes) {
            changes.forEach(function (change) { change.addedNodes.forEach(enhanceWithin); });
        }).observe(document.body, { childList: true, subtree: true });

        // Bind clicks on any input with data-hrm-datepicker or class hrm-date-input
        $(document).on('click focus', '[data-hrm-datepicker], .hrm-date-input', function (e) {
            if (this.disabled || this.readOnly) return;
            showOverlay(this);
        });
    });

    // Fix: Xử lý an toàn mở modal duyệt bảng lương và phát hành phiếu lương (tránh xung đột data-toggle và onclick)
    document.addEventListener('click', function (e) {
        var btn = e.target && e.target.closest ? e.target.closest('.btn-payroll-publish, .btn-payroll-approve, .btn-payroll-reject, .btn-payroll-finalize, [data-target="#modalBatchPublish"], [data-target="#modalBatchApprove"], [data-target="#modalBatchReject"], [data-target="#modalBatchFinalize"], [data-target="#modalAddAllowance"], [data-target="#modalAddDeduction"], [data-target="#modalAddAdvance"]') : null;
        if (!btn) return;
        // Nút gửi biểu mẫu (ví dụ Tính lương tự động) phải chạy đúng thao tác của nó, không mở hộp duyệt.
        if (btn.type === 'submit') return;

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


