# Đăng ký khuôn mặt chấm công

## Phạm vi bản hiện tại

Đã có đăng ký ảnh và HR/Admin duyệt để đối soát danh tính. Có thể bật **thử so khớp OpenCV nội bộ** trên từng lượt chấm ngoài công ty; đây chỉ là điểm tương đồng ảnh, không kiểm tra người thật (liveness) và không tự duyệt công. Không có API nhận diện bên thứ ba hoặc tài khoản/API key AWS trong bản triển khai này.

1. Nhân viên mở **Hồ sơ của tôi → Khuôn mặt chấm công** (`/FaceEnrollment`), đọc thông tin sử dụng ảnh, đồng ý và chụp ảnh trực tiếp.
2. HR/Admin vào cùng trang, đối chiếu người đăng ký với hồ sơ nhân sự, rồi duyệt hoặc từ chối kèm ghi chú. Không tự duyệt đăng ký của mình.
3. Trạng thái `ACTIVE` nghĩa là **mẫu đã được HR duyệt**. Nhân viên mới được gửi lượt vào/ra ngoài công ty sau thời điểm mẫu được duyệt. Ảnh chụp từ trước thời điểm này phải chụp lại.
4. Mỗi lượt vào/ra vẫn **chờ xác minh thủ công**, kể cả khi OpenCV báo đạt ngưỡng, GPS và lịch hợp lệ. Chỉ lượt được duyệt mới vào bảng công. Lượt ghé khách hàng không tạo giờ vào/ra và không bắt buộc mẫu khuôn mặt.
5. Muốn đăng ký lại, nhân viên thu hồi mẫu hiện tại rồi gửi mẫu mới và chờ duyệt. Chủ tài khoản hoặc HR/Admin được thu hồi; tối đa một mẫu đang chờ/đang hoạt động theo luồng ứng dụng. Mỗi lần gửi cách nhau ít nhất 60 giây, tối đa 3 lần trong 24 giờ.

Thu hồi mẫu chặn phê duyệt các lượt mới đang chờ gắn với mẫu đó; nhân viên phải đăng ký và chấm lại. Các lượt đã được duyệt và dữ liệu HANET cũ không bị tự xóa hay tính lại. Chấm offline không được xem là đã vượt qua liveness; server vẫn kiểm tra thời điểm chụp, mẫu và quyền khi đồng bộ.

## Lưu trữ và vận hành

- Ảnh đăng ký JPEG tối đa 2 MB được mã hóa bằng ASP.NET Data Protection trước khi ghi `HrmFaceEnrollment.PhotoProtected`. Không dùng ảnh đại diện làm mẫu và không đặt ảnh trong thư mục tĩnh.
- Trang đăng ký/endpoint ảnh mẫu chỉ cho chủ tài khoản và HR/Admin xem. Khi xét một lượt chấm cụ thể, người có quyền duyệt lượt đó có thể xem đúng mẫu `ACTIVE` đã liên kết qua `/RemoteAttendance/FaceReference?id=<mã lượt chấm>`; quyền này không mở danh sách mẫu của phòng hoặc toàn công ty, không trả mẫu đã thu hồi/từ chối. Phản hồi ảnh không cache. Server ghi phiên bản đồng ý, thời điểm, người xử lý và nhật ký thay đổi.
- Từ chối hoặc thu hồi sẽ xóa nội dung ảnh mẫu trong bảng hoạt động (`PhotoProtected=NULL`), giữ lịch sử quyết định. Điều này không xóa ảnh chứng từ chấm công riêng hoặc bản sao lưu đã tồn tại; thời hạn giữ/xóa backup cần nằm trong quy trình vận hành.
- Khóa Data Protection mặc định được giữ trong `HrmDataProtectionKey` của SQL. Nếu cấu hình `HRM_DATA_PROTECTION_PATH`, đường dẫn phải nằm trên ổ lưu bền vững và được sao lưu. Không xóa kho khóa hoặc đổi `SetApplicationName("NHIGIA.Modern.v1")` khi deploy; mất khóa sẽ không đọc được ảnh đã mã hóa.
- Bảo vệ và sao lưu cả dữ liệu lẫn kho khóa; mã hóa ứng dụng không thay thế kiểm soát quyền SQL hay mã hóa backup. Khi dùng kho khóa trong cùng database, không coi đây là bảo vệ khỏi người đã có toàn quyền database.
- Startup chạy migration bổ sung `App_Data/face-enrollment.sql` sau bảng chấm công ngoài công ty, rồi chạy `App_Data/opencv-face-trial.sql` để thêm cột lưu sự đồng ý và kết quả thử. Tài khoản triển khai phải có quyền tạo bảng, chỉ mục và bổ sung cột; không thay đổi dữ liệu chấm công hiện có.

Nguồn triển khai: `NHIGIA.Modern/Controllers/FaceEnrollmentController.cs`, `Infrastructure/HrmDataStore.FaceEnrollment.cs`, `Infrastructure/HrmDataStore.RemoteAttendance.cs`, `Infrastructure/OpenCvFaceMatcher.cs`, `App_Data/face-enrollment.sql`, `App_Data/opencv-face-trial.sql` và cấu hình Data Protection trong `Program.cs`. Hướng dẫn giới hạn và cấu hình chạy OpenCV nằm tại [`docs/opencv-face-trial.md`](opencv-face-trial.md).

## Giai đoạn tự xác thực có liveness: phương án sau thử nghiệm

Nếu sau này doanh nghiệp cần tự xác minh người thật và tự động quyết định công, cần một dịch vụ liveness chuyên dụng kết hợp so khớp 1:1. **Amazon Rekognition Face Liveness + CompareFaces** là một phương án có tính phí, chưa được tích hợp ở đây. Bản OpenCV hiện tại chỉ là thử nghiệm miễn phí theo lượt gọi, chạy trên máy chủ HRM; máy chủ vẫn phát sinh chi phí tài nguyên.

Luồng dự kiến: backend tạo phiên gắn với người dùng/mục đích → thành phần `FaceLivenessDetector` thu video → backend đọc kết quả trực tiếp từ AWS → so khớp ảnh tham chiếu với mẫu đã duyệt → kiểm tra GPS/lịch và ghi công. Khi bắt đầu dùng AI, mẫu thủ công hiện có cần được đăng ký/xác minh lại qua luồng liveness trước khi bật duyệt tự động.

Chuẩn bị để tích hợp:

- Tài khoản AWS doanh nghiệp, thanh toán và một vùng hỗ trợ Face Liveness; kiểm tra quota/giá trong vùng lựa chọn và ngân sách cảnh báo. Danh sách hiện tại có Thailand, Malaysia, Tokyo và Mumbai.
- Quyền backend cho `CreateFaceLivenessSession`, `GetFaceLivenessSessionResults` và `CompareFaces`. Trình duyệt chỉ nhận credentials tạm thời cho phiên; không gửi khóa AWS dài hạn xuống JavaScript hoặc ghi vào Git.
- Nhúng thành phần React của Amplify vào trang Razor. Có thể dùng custom credentials provider với STS, giữ nguyên hệ thống đăng nhập HRM; không bắt buộc chuyển người dùng sang Cognito.
- Ràng buộc phiên với user, mẫu và lượt chấm; đọc kết quả tại server, chống dùng lại. Phiên AWS dùng một lần, hết hạn sau 3 phút; callback từ trình duyệt không phải bằng chứng xác minh thành công.
- Kiểm thử camera, ánh sáng, mạng và ngưỡng liveness/độ tương đồng trên điện thoại thực. Giữ luồng xác minh thủ công khi dịch vụ lỗi hoặc kết quả không đạt; không tự chấp nhận chỉ vì có ảnh/GPS.
- Chốt nơi lưu trữ, phạm vi truy cập và thời hạn giữ ảnh trước khi gửi dữ liệu thật sang nhà cung cấp. Nếu dùng S3 cho ảnh tham chiếu/audit, bucket phải cùng vùng với phiên liveness; AWS cũng hỗ trợ trả ảnh dạng bytes.

Nếu ưu tiên hỗ trợ trong nước, có thể lấy báo giá **Web liveness + so khớp 1:1** từ FPT AI hoặc VNPT, yêu cầu sandbox và tài liệu chính thức trước khi làm adapter. Không cần thêm OCR/NFC giấy tờ chỉ để chấm công. Azure Face không phải lựa chọn mặc định ở giai đoạn này vì nhận diện/liveness cần xét duyệt quyền truy cập và SDK.

Tài liệu chính thức đối chiếu ngày 30/09/2026:

- [AWS: quy trình API Face Liveness](https://docs.aws.amazon.com/rekognition/latest/dg/face-liveness-programming-api.html)
- [AWS: phiên dùng một lần và thời hạn](https://docs.aws.amazon.com/rekognition/latest/APIReference/API_CreateFaceLivenessSession.html)
- [Amplify: thành phần web và credentials provider](https://ui.docs.amplify.aws/react/connected-components/liveness)
- [AWS: CompareFaces](https://docs.aws.amazon.com/rekognition/latest/dg/faces-comparefaces.html) và [vùng hỗ trợ](https://docs.aws.amazon.com/rekognition/latest/dg/face-liveness-faq.html)
- [FPT: Liveness v3](https://docs-vision.fpt.ai/ekyc/III-integration/III-2-APIs/b-APIs%20of%20AI%20Engine/liveness_api_v3/), [VNPT: công nghệ và SDK](https://ekyc.vnpt.vn/vi/technology)
- [Azure: điều kiện truy cập Face](https://learn.microsoft.com/en-us/azure/foundry/responsible-ai/computer-vision/limited-access-identity)
