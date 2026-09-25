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
        if (role === 'MANAGER' && (item.RoleCode !== 'EMPLOYEE' || item.StatusCode !== 'PENDING_MANAGER' || !departmentId ||
            Number(item.DepartmentId) !== Number(departmentId))) return false;
        return item.StatusCode === 'PENDING_MANAGER' || item.StatusCode === 'PENDING_HR';
    }

    root.HrmRequestForm = { escapeHtml: escapeHtml, employeeLabel: employeeLabel, employeeId: employeeId,
        populateEmployees: populateEmployees, fileError: fileError, dateRangeError: dateRangeError,
        validate: validate, canApprove: canApprove };
    if (typeof module !== 'undefined' && module.exports) module.exports = root.HrmRequestForm;
})(typeof window !== 'undefined' ? window : globalThis);
