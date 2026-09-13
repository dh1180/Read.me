using Microsoft.EntityFrameworkCore;
using ReadMeApp.Models;

namespace ReadMeApp.Data;

public class ReadmeDbContext : DbContext
{
    public ReadmeDbContext(DbContextOptions<ReadmeDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<UserBook> UserBooks => Set<UserBook>();
    public DbSet<ReadingNote> ReadingNotes => Set<ReadingNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => b.Isbn);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Author).HasMaxLength(100);
            entity.Property(b => b.Publisher).HasMaxLength(100);
            entity.Property(b => b.CoverImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<UserBook>(entity =>
        {
            entity.HasKey(ub => ub.Id);
            entity.HasOne(ub => ub.Book)
                  .WithMany(b => b.UserBooks)
                  .HasForeignKey(ub => ub.BookId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReadingNote>(entity =>
        {
            entity.HasKey(rn => rn.Id);
            entity.HasOne(rn => rn.UserBook)
                  .WithMany(ub => ub.Notes)
                  .HasForeignKey(rn => rn.UserBookId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public static void SeedSampleData(ReadmeDbContext context)
    {
        if (context.UserBooks.Any()) return;

        var book1 = new Book
        {
            Isbn = "9788960518650",
            Title = "소크라테스 익스프레스",
            Author = "에릭 와이너",
            Publisher = "어크로스",
            CoverImageUrl = "https://search1.kakaocdn.net/thumb/R120x174.q85/?fname=http%3A%2F%2Ft1.daumcdn.net%2Flbook%2Fimage%2F5670857%3Ftimestamp%3D20240320152643",
            Description = "마르쿠스 아우렐리우스부터 몽테뉴까지 역사상 가장 위대한 철학자들을 만나러 떠나는 여행기."
        };

        var book2 = new Book
        {
            Isbn = "9791161571188",
            Title = "불편한 편의점",
            Author = "김호연",
            Publisher = "나무옆의자",
            CoverImageUrl = "https://image.aladin.co.kr/product/26942/70/cover500/k612730088_1.jpg",
            Description = "서울역 노숙자 독고 씨가 청파동의 작은 편의점 야간 알바를 맡으며 일어나는 유쾌하고 따스한 이야기."
        };

        var book3 = new Book
        {
            Isbn = "9788998441012",
            Title = "모순",
            Author = "양귀자",
            Publisher = "쓰다",
            CoverImageUrl = "https://image.aladin.co.kr/product/2584/35/cover500/8998441014_1.jpg",
            Description = "인생은 살아가면서 탐구하는 것. 안진진의 시선으로 바라본 인생의 모순과 선택의 무게."
        };

        var book4 = new Book
        {
            Isbn = "9788966263301",
            Title = "클린 코드 (Clean Code)",
            Author = "로버트 C. 마틴",
            Publisher = "인사이트",
            CoverImageUrl = "https://image.aladin.co.kr/product/3084/64/cover500/8966260956_1.jpg",
            Description = "애자일 소프트웨어 장인 정신과 읽기 쉬운 깨끗한 코드 작성법을 다룬 바이블."
        };

        context.Books.AddRange(book1, book2, book3, book4);
        context.SaveChanges();

        var review1 = new UserBook
        {
            BookId = book1.Id,
            ReviewerName = "지혜로운산책자",
            Rating = 5,
            Summary = "나이 듦과 삶의 태도를 철학자들의 시선으로 유쾌하게 풀어낸 책",
            Quote = "우리는 질문을 멈출 때 늙기 시작한다. 호기심은 노화를 막는 유일한 백신이다.",
            Content = "철학이란 책상머리에 앉아 하는 어려운 학문인 줄만 알았는데, 기차를 타고 철학자들의 발자취를 따라가며 일상 속 지혜를 배울 수 있어 너무 좋았습니다. 특히 마르쿠스 아우렐리우스 편은 번아웃이 올 때마다 다시 꺼내 읽고 싶습니다.",
            ReadDate = DateTime.UtcNow.AddDays(-2),
            LikesCount = 14,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Status = ReadingStatus.Completed
        };

        var review2 = new UserBook
        {
            BookId = book2.Id,
            ReviewerName = "따뜻한라떼",
            Rating = 5,
            Summary = "퇴근길 지하철에서 혼자 눈물 훔치게 만든 힐링 소설",
            Quote = "결국 삶은 관계였고 관계는 소통이었다. 행복은 멀리 있지 않고 내 옆 사람의 말을 들어주는 데 있었다.",
            Content = "편의점이라는 가장 차갑고 계산적인 공간이 사람들의 온기로 채워지는 과정이 너무나 감동적이었습니다. 독고 씨의 서툰 따뜻함이 제 지친 일상에도 큰 위로가 되었습니다. 가볍게 읽기 시작했다가 오래 기억에 남을 책입니다.",
            ReadDate = DateTime.UtcNow.AddDays(-5),
            LikesCount = 28,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            Status = ReadingStatus.Completed
        };

        var review3 = new UserBook
        {
            BookId = book3.Id,
            ReviewerName = "새벽네시",
            Rating = 5,
            Summary = "내 20대 인생의 방향을 다시 생각해보게 한 명작",
            Quote = "인생은 살아가면서 탐구하는 것이지, 탐구하면서 살아가는 것이 아니다.",
            Content = "주인공 안진진의 선택을 보며 인생의 모순이란 피해야 할 대상이 아니라 껴안고 살아가야 할 본질이라는 걸 배웠습니다. 문장 하나하나가 가슴에 박히듯 아름답고 울림이 큽니다. 친구들에게도 꼭 권하고 싶어요.",
            ReadDate = DateTime.UtcNow.AddDays(-10),
            LikesCount = 19,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            Status = ReadingStatus.Completed
        };

        var review4 = new UserBook
        {
            BookId = book4.Id,
            ReviewerName = "기록하는개발자",
            Rating = 4,
            Summary = "더 좋은 코드를 넘어 더 나은 태도를 갖추게 해주는 책",
            Quote = "캠핑장을 떠날 때는 처음 왔을 때보다 더 깨끗하게 남겨두어라.",
            Content = "코드뿐만 아니라 우리가 살아가는 모든 작업과 협업의 기본을 말해주는 책입니다. 함수 하나, 변수명 하나에도 정성을 다해야 하는 이유를 배웠습니다.",
            ReadDate = DateTime.UtcNow.AddDays(-14),
            LikesCount = 9,
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            Status = ReadingStatus.Completed
        };

        context.UserBooks.AddRange(review1, review2, review3, review4);
        context.SaveChanges();
    }
}
