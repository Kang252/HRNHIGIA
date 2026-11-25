(function (root) {
    'use strict';

    function escapeHtml(value) {
        return String(value == null ? '' : value).replace(/[&<>"']/g, function (character) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[character];
        });
    }

    function employeeLabel(user) {
        return (user.DisplayName || user.Username || 'Nhân viên') +
            (user.DepartmentName ? ' · ' + user.DepartmentName : '') +
            ' · ' + (user.EmployeeCode || user.Username || ('#' + user.Id));
    }

    function employeeId(value, users, currentId, currentName) {
        var normalized = String(value || '').trim().toLocaleLowerCase('vi');
        if (!normalized) return null;
        // Keep the default current employee stable while the directory is still loading.
        if (normalized === String(currentName || '').trim().toLocaleLowerCase('vi')) return currentId;
        var matches = (users || []).filter(function (user) {
            return user.IsActive !== false && [employeeLabel(user), user.DisplayName, user.Username, user.EmployeeCode]
                .some(function (label) { return label && String(label).trim().toLocaleLowerCase('vi') === normalized; });
        });
        // A duplicate display name is ambiguous: require the full label or employee code.
        return matches.length === 1 ? matches[0].Id : null;
    }

    function populateEmployees(users) {
        var list = document.getElementById('dynEmployeeOptions');
        if (!list) return;
        list.innerHTML = (users || []).filter(function (user) { return user.IsActive !== false; })
            .map(function (user) { return '<option value="' + escapeHtml(employeeLabel(user)) + '"></option>'; }).join('');
    }

    function fileError(file) {
        if (!file) return '';
        if (file.size === 0) return 'Tệp đính kèm đang trống. Vui lòng chọn tệp có nội dung.';
        if (file.size > 10 * 1024 * 1024) return 'Tệp đính kèm không được vượt quá 10 MB.';
        if (!/\.(pdf|jpe?g|png|docx?|xlsx?)$/i.test(file.name || ''))
            return 'Chỉ hỗ trợ tệp PDF, JPG, PNG, DOC, DOCX, XLS hoặc XLSX.';
        return '';
    }

    function dateRangeError(start, end) {
        if (!start || !end) return 'Vui lòng chọn đầy đủ ngày bắt đầu và ngày kết thúc.';
        if (end < start) return 'Ngày kết thúc hoặc hạn xử lý không được trước ngày bắt đầu.';
        return '';
    }

    function validate(form, users, currentId, currentName, file) {
        var employeeInput = form.querySelector('#dynEmployeeInput');
        var id = employeeId(employeeInput.value, users, currentId, currentName);
        if (!id) {
            employeeInput.focus();
            return 'Vui lòng chọn đúng nhân viên trong danh sách gợi ý; tên trùng cần chọn kèm mã nhân viên.';
        }
        form.querySelector('#dynEmployeeId').value = id;
        var inputs = form.querySelectorAll('input, select, textarea');
        for (var i = 0; i < inputs.length; i++) {
            if (!inputs[i].checkValidity()) {
                inputs[i].reportValidity();
                return 'Vui lòng kiểm tra các trường bắt buộc và giá trị số trong biểu mẫu.';
            }
        }
        return fileError(file);
    }

    function canApprove(item, role, actorId, departmentId) {
        if (!item || !['ADMIN', 'HR', 'DIRECTOR', 'MANAGER'].includes(role)) return false;
        if (Number(item.UserId) === Number(actorId)) return false;
        if (role === 'ADMIN') return item.StatusCode === 'PENDING_MANAGER' || item.StatusCode === 'PENDING_HR';
        if (role === 'MANAGER') return item.RoleCode === 'EMPLOYEE' && item.StatusCode === 'PENDING_MANAGER' &&
            !!departmentId && Number(item.DepartmentId) === Number(departmentId);
        return item.StatusCode === 'PENDING_HR';
    }

    // Workdays and approver come from the server so the preview cannot drift from CreateLeave.
    var previewUrl = null, previewRequest = 0, previewDays = null;

    function formatDays(value) {
        return String(Math.round(Number(value) * 10) / 10).replace('.', ',') + ' ngày công';
    }

    function leaveDaysText() { return previewDays === null ? '' : formatDays(previewDays); }

    function refreshPreview() {
        var form = document.querySelector('#dynamicFormContainer .tc-tanca-form');
        if (!form || !previewUrl) return;
        var value = function (id) { var input = form.querySelector('#' + id); return input ? input.value : ''; };
        var days = form.querySelector('#dynLeaveDaysCalc'), approver = form.querySelector('#dynApproverPreview');
        var show = function (daysText, approverText) {
            if (days) days.textContent = daysText;
            if (approver) approver.textContent = approverText;
        };
        var current = ++previewRequest, query = { EmployeeId: value('dynEmployeeId') };
        previewDays = null;
        if (!query.EmployeeId) { show('—', 'Chọn nhân viên để xem người duyệt'); return; }
        if (form.getAttribute('data-template') === 'LEAVE') {
            query.StartDate = value('dynStartDate');
            query.EndDate = value('dynEndDate');
            query.SessionCode = value('dynSession');
            if (dateRangeError(query.StartDate, query.EndDate)) { show('—', 'Chọn khoảng ngày hợp lệ'); return; }
        }
        show('Đang tính…', 'Đang xác định người duyệt…');
        root.jQuery.get(previewUrl, query).done(function (response) {
            if (current !== previewRequest) return;
            var data = response.Data || {};
            if (query.StartDate) previewDays = data.RequestedDays;
            show(formatDays(data.RequestedDays || 0), (data.ApproverLabel || 'Người có thẩm quyền') + ' phê duyệt');
        }).fail(function (xhr) {
            if (current !== previewRequest) return;
            show('Chưa tính được', (xhr.responseJSON && xhr.responseJSON.Message) || 'Chưa xác định được người duyệt');
        });
    }

    function bindPreview(url) {
        previewUrl = url;
        root.jQuery(document).off('.leavePreview').on('change.leavePreview', '#dynStartDate, #dynEndDate, #dynSession, #dynEmployeeInput', function () {
            setTimeout(refreshPreview, 0); // after the handler that resolves dynEmployeeId
        });
    }

    root.HrmRequestForm = { bindPreview: bindPreview, refreshPreview: refreshPreview, leaveDaysText: leaveDaysText, formatDays: formatDays,
         escapeHtml: escapeHtml, employeeLabel: employeeLabel, employeeId: employeeId,
        populateEmployees: populateEmployees, fileError: fileError, dateRangeError: dateRangeError,
        validate: validate, canApprove: canApprove };
    if (typeof module !== 'undefined' && module.exports) module.exports = root.HrmRequestForm;
})(typeof window !== 'undefined' ? window : globalThis);
