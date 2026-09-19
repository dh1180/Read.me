<div align="center">

# 📖 Read.me

### 읽은 책과 마음에 남은 문장을 기록하고 나누는 독서 커뮤니티

**책을 찾고, 읽고 싶은 책을 모으고, 독서록을 작성하고, 다른 독자의 기록을 둘러볼 수 있는 웹 서비스입니다.**

</div>

---

## 서비스 소개

Read.me는 개발자만을 위한 도구가 아니라 **누구나 편하게 사용할 수 있는 보편적인 독서 기록 서비스**를 목표로 합니다.

카카오 도서 검색으로 책을 찾은 뒤 읽고 싶은 책에 담거나 독서록을 작성할 수 있습니다. 작성한 기록은 개인 공간에서 관리하고, 완성된 독서록은 커뮤니티 피드에서 다른 독자들과 공유할 수 있습니다.

```text
책 찾기 → 읽고 싶은 책에 담기 → 독서록 작성 → 내 기록 관리 → 커뮤니티 공유
```

## 주요 기능

- **도서 검색**: 카카오 도서 검색 API 기반 제목·저자 검색
- **읽고 싶은 책**: 아직 읽지 않은 책을 개인 기록에 보관
- **독서록 작성**: 별점, 한 줄 평, 인상 깊은 문장, 감상 본문 작성
- **내 독서 기록**: 전체 / 읽고 싶은 책 / 읽는 중 / 완독 상태별 관리
- **모두의 독서록**: 최신순·공감순·별점순 탐색과 통합 검색
- **독서 메모**: 페이지 번호, 인용문, 생각을 Markdown으로 기록
- **카카오 로그인**: 개인 기록 관리와 작성자 권한 확인
- **README 내보내기**: 원하는 사용자를 위한 선택적 Markdown 내보내기 기능

## 이번 개선 사항

- 보라색 SaaS 스타일에서 **크림·딥그린 기반의 독서 서비스 디자인**으로 개편
- 내 기록과 커뮤니티 피드를 명확히 분리
- 읽고 싶은 책이 커뮤니티 독서록으로 노출되던 구조 수정
- 로그인 사용자만 작성·삭제·메모 관리 가능하도록 권한 강화
- 본인의 독서 기록만 삭제하고 메모를 편집할 수 있도록 소유권 검사 추가
- 전역 CSRF 검증과 Ajax antiforgery token 적용
- Markdown raw HTML 비활성화로 XSS 위험 완화
- 동일 브라우저의 반복 공감 방지
- 홈 피드 및 내 기록 페이지네이션 적용
- README 내보내기를 로그인한 사용자의 기록으로 제한

## 기술 스택

- **Backend**: C#, .NET 10, ASP.NET Core MVC, Entity Framework Core
- **Database**: SQL Server / SQLite
- **Frontend**: Razor Views, Bootstrap, JavaScript, jQuery / Ajax
- **Integration**: Kakao Book Search API, Kakao OAuth 2.0
- **Deploy**: Azure App Service, GitHub Actions
- **Markdown**: Markdig

## 프로젝트 구조

```text
Read.me/
├── Controllers/
├── Data/
├── Models/
│   └── ViewModels/
├── Services/
├── Views/
│   ├── Books/
│   ├── Home/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   └── js/
├── Program.cs
└── ReadMeApp.csproj
```

## 로컬 실행

```bash
git clone https://github.com/dh1180/Read.me.git
cd Read.me
dotnet restore
dotnet run
```

SQLite로 실행하려면 환경변수 `DatabaseProvider=Sqlite`를 사용할 수 있습니다.

카카오 도서 검색 및 로그인 기능을 사용하려면 다음 환경변수를 설정합니다.

```text
Kakao__RestApiKey
Kakao__RedirectUri
```

## 보안 및 데이터 모델 참고

현재 배포 중인 기존 데이터베이스와의 호환성을 유지하기 위해 작성자 소유권은 로그인 닉네임과 기존 `ReviewerName`을 연결하여 검사합니다. 장기적으로는 EF Core Migration을 도입하고 별도의 사용자 테이블과 불변 사용자 ID를 `UserBook`에 연결하는 구조로 확장할 예정입니다.

---

<div align="center">

**Read.me**  
책을 읽고, 기억하고, 함께 나누는 곳

</div>
