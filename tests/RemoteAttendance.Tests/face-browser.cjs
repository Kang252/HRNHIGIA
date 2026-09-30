const assert = require('node:assert/strict');
const path = require('node:path');

module.exports = async function testFaceEnrollment(browser, origin) {
    assert(/^http:\/\/127\.0\.0\.1:\d+$/.test(origin), 'face tests require an isolated loopback host');
    assert(process.env.HRM_FACE_TEST_COOKIE && process.env.HRM_FACE_REVIEW_COOKIE, 'face test employee and HR cookies required');
    const errors = [], output = process.env.HRM_REMOTE_TEST_OUTPUT;
    const employee = await browser.newContext({viewport: {width: 390, height: 844}, permissions: ['camera']});
    const hr = await browser.newContext({viewport: {width: 1440, height: 1000}});
    try {
        await employee.addCookies([{name: 'NHIGIA.Auth.v3', value: process.env.HRM_FACE_TEST_COOKIE, url: origin}]);
        await hr.addCookies([{name: 'NHIGIA.Auth.v3', value: process.env.HRM_FACE_REVIEW_COOKIE, url: origin}]);
        const page = await employee.newPage(), review = await hr.newPage();
        page.on('pageerror', error => errors.push(error.message));
        review.on('pageerror', error => errors.push(error.message));
        page.on('dialog', dialog => dialog.accept());
        review.on('dialog', dialog => dialog.accept());
        await page.goto(origin + '/FaceEnrollment');
        await page.waitForFunction(() => document.querySelector('#faceStatus').textContent === 'Chưa đăng ký');
        assert.equal(await page.locator('#faceReviewPanel').isVisible(), false, 'employee cannot see reviewer panel');
        assert.equal(await page.locator('input[type=file]').count(), 0, 'registration captures camera directly');
        assert(await page.locator('#faceSubmit').isDisabled(), 'no submission without capture and consent');
        assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth + 1), 'mobile overflow');

        // A denied camera leaves a useful error and never enables submission.
        await page.evaluate(() => {
            window.faceTestCamera = navigator.mediaDevices.getUserMedia.bind(navigator.mediaDevices);
            navigator.mediaDevices.getUserMedia = () => Promise.reject(new DOMException('denied', 'NotAllowedError'));
        });
        await page.click('#faceOpenCamera');
        await page.waitForFunction(() => document.querySelector('#faceMessage').textContent.includes('Camera chưa được cấp quyền'));
        assert(await page.locator('#faceSubmit').isDisabled());
        await page.evaluate(() => { navigator.mediaDevices.getUserMedia = window.faceTestCamera; });

        async function openCamera() {
            await page.click('#faceOpenCamera');
            await page.waitForFunction(() => {
                const video = document.querySelector('#faceVideo');
                return !document.querySelector('#faceCapture').disabled && video.videoWidth > 0;
            });
        }

        // Exercise the hidden-page and pagehide handlers, checking tracks have actually ended.
        await openCamera();
        assert(await page.evaluate(() => {
            const tracks = document.querySelector('#faceVideo').srcObject.getTracks();
            Object.defineProperty(document, 'hidden', {configurable: true, value: true});
            document.dispatchEvent(new Event('visibilitychange'));
            delete document.hidden;
            return tracks.every(track => track.readyState === 'ended') && document.querySelector('#faceVideo').srcObject === null;
        }), 'camera must stop when page becomes hidden');
        await openCamera();
        assert(await page.evaluate(() => {
            const tracks = document.querySelector('#faceVideo').srcObject.getTracks();
            window.dispatchEvent(new Event('pagehide'));
            return tracks.every(track => track.readyState === 'ended') && document.querySelector('#faceVideo').srcObject === null;
        }), 'camera must stop on pagehide');

        await openCamera();
        await page.click('#faceCapture');
        await page.waitForFunction(() => !document.querySelector('#facePreview').hidden);
        assert(await page.locator('#faceSubmit').isDisabled(), 'capture alone is not consent');
        assert(await page.evaluate(() => document.querySelector('#faceVideo').srcObject === null), 'capture releases camera');
        await page.check('#faceConsent');
        assert(await page.locator('#faceSubmit').isEnabled());
        await page.evaluate(() => scrollTo(0, 0));
        await page.screenshot({path: path.join(output, 'face-registration-mobile.png'), fullPage: true, animations: 'disabled'});
        const submitted = page.waitForResponse(response => response.url().endsWith('/FaceEnrollment/Submit') && response.request().method() === 'POST');
        await page.click('#faceSubmit');
        assert.equal((await submitted).status(), 200);
        await page.waitForFunction(() => document.querySelector('#faceStatus').dataset.status === 'PENDING');
        assert(await page.locator('#faceOpenCamera').isDisabled(), 'pending enrollment prevents replacement');
        assert.equal(await page.locator('#facePreview').isVisible(), false, 'submission clears unsent capture');
        const state = await (await employee.request.get(origin + '/FaceEnrollment/State')).json();
        const enrollmentId = state.Data.Enrollment.Id;
        assert.equal(state.Data.Enrollment.StatusCode, 'PENDING');
        assert((await employee.request.get(origin + '/FaceEnrollment/Photo?id=' + enrollmentId)).ok());

        await review.goto(origin + '/FaceEnrollment');
        const note = review.locator('#faceReviewNote-' + enrollmentId);
        await note.waitFor();
        const card = review.locator('.face-review-item').filter({has: note});
        await card.locator('img').evaluate(image => image.decode());
        await card.getByRole('button', {name: 'Duyệt mẫu', exact: true}).click();
        assert((await review.locator('#faceMessage').textContent()).includes('Nhập ghi chú'), 'review must require a note');
        await note.fill('Đã đối chiếu nhân viên trong kiểm thử trình duyệt.');
        if (await review.evaluate(() => document.body.classList.contains('hrm-menu-collapsed'))) await review.click('.hrm-menu-toggle');
        await review.waitForFunction(() => Math.abs(document.querySelector('#leftsidebar').getBoundingClientRect().left) < 1);
        assert(await review.evaluate(() => document.documentElement.scrollWidth <= innerWidth + 1), 'desktop reviewer overflow');
        await review.evaluate(() => scrollTo(0, 0));
        await review.screenshot({path: path.join(output, 'face-review-desktop.png'), fullPage: true, animations: 'disabled'});
        const approved = review.waitForResponse(response => response.url().endsWith('/FaceEnrollment/Review') && response.request().method() === 'POST');
        await card.getByRole('button', {name: 'Duyệt mẫu', exact: true}).click();
        assert.equal((await approved).status(), 200);
        await review.waitForFunction(id => !document.querySelector('#faceReviewNote-' + id), enrollmentId);
        await page.click('#faceRefresh');
        await page.waitForFunction(() => document.querySelector('#faceStatus').dataset.status === 'ACTIVE');
        assert((await page.locator('#faceStatusDetail').textContent()).includes('chờ quản lý'), 'approval must not claim automated verification');
        assert(await page.locator('#faceOpenCamera').isDisabled(), 'active enrollment prevents direct replacement');

        await review.fill('#faceSearch', 'face_browser_employee');
        const searched = review.waitForResponse(response => response.url().includes('/FaceEnrollment/State?search='));
        await review.locator('#faceSearchForm').getByRole('button', {name: 'Tìm kiếm'}).click();
        assert.equal((await searched).status(), 200);
        await review.waitForFunction(() => document.querySelectorAll('#faceActive .face-review-item').length === 1);
        const revokeNote = review.locator('#faceActiveNote-' + enrollmentId);
        const activeCard = review.locator('#faceActive .face-review-item').filter({has: revokeNote});
        await activeCard.getByRole('button', {name: 'Thu hồi mẫu'}).click();
        assert((await review.locator('#faceMessage').textContent()).includes('Nhập lý do'), 'HR revocation requires reason');
        await revokeNote.fill('HR thu hồi mẫu kiểm thử sau khi xác minh luồng đăng ký.');
        const revoked = review.waitForResponse(response => response.url().endsWith('/FaceEnrollment/Revoke') && response.request().method() === 'POST');
        await activeCard.getByRole('button', {name: 'Thu hồi mẫu'}).click();
        assert.equal((await revoked).status(), 200);
        await page.click('#faceRefresh');
        await page.waitForFunction(() => document.querySelector('#faceStatus').dataset.status === 'REVOKED');
        assert(await page.locator('#faceOpenCamera').isEnabled(), 'revoked employee can start fresh registration');
        assert.equal(await page.locator('#faceReference').isVisible(), false, 'revocation removes reference photo');
        assert.equal((await employee.request.get(origin + '/FaceEnrollment/Photo?id=' + enrollmentId)).status(), 404, 'revocation purges stored photo');
        assert.deepEqual(errors, []);
        console.log('PASS face capture+consent, pending/HR approval/revoke, protected photo purge, camera cleanup, mobile/desktop layouts');
    } finally {
        await employee.close();
        await hr.close();
    }
};
