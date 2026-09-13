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

        // Book configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => b.Isbn).IsUnique();
            entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Author).HasMaxLength(100);
            entity.Property(b => b.Publisher).HasMaxLength(100);
            entity.Property(b => b.CoverImageUrl).HasMaxLength(500);
        });

        // UserBook configuration
        modelBuilder.Entity<UserBook>(entity =>
        {
            entity.HasKey(ub => ub.Id);
            entity.HasOne(ub => ub.Book)
                  .WithMany(b => b.UserBooks)
                  .HasForeignKey(ub => ub.BookId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ReadingNote configuration
        modelBuilder.Entity<ReadingNote>(entity =>
        {
            entity.HasKey(rn => rn.Id);
            entity.HasOne(rn => rn.UserBook)
                  .WithMany(ub => ub.Notes)
                  .HasForeignKey(rn => rn.UserBookId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(rn => rn.Thought).IsRequired();
        });
    }

    public static void SeedSampleData(ReadmeDbContext context)
    {
        if (context.Books.Any()) return;

        var book1 = new Book
        {
            Isbn = "9788966263301",
            Title = "클린 코드 (Clean Code)",
            Author = "로버트 C. 마틴",
            Publisher = "인사이트",
            TotalPages = 584,
            CoverImageUrl = "https://image.aladin.co.kr/product/3084/64/cover500/8966260956_1.jpg",
            Description = "애자일 소프트웨어 장인 정신과 읽기 쉬운 깨끗한 코드 작성법을 다룬 바이블."
        };

        var book2 = new Book
        {
            Isbn = "9788966262472",
            Title = "프로그래머의 길, 멘토에게 묻다",
            Author = "데이브 후버, 아디트야 바르가바",
            Publisher = "인사이트",
            TotalPages = 340,
            CoverImageUrl = "https://image.aladin.co.kr/product/642/41/cover500/8966260271_1.jpg",
            Description = "소프트웨어 장인이 되기 위해 스스로를 훈련하고 성장하는 구체적 실천법."
        };

        var book3 = new Book
        {
            Isbn = "9788968482954",
            Title = "도메인 주도 설계 핵심",
            Author = "반 버논",
            Publisher = "에이콘출판",
            TotalPages = 288,
            CoverImageUrl = "https://image.aladin.co.kr/product/10452/78/cover500/8968482950_1.jpg",
            Description = "복잡한 비즈니스 로직을 소프트웨어 모델로 풀어내는 DDD의 정수."
        };

        context.Books.AddRange(book1, book2, book3);
        context.SaveChanges();

        var ub1 = new UserBook
        {
            BookId = book1.Id,
            Status = ReadingStatus.Reading,
            CurrentPage = 280,
            StartDate = DateTime.UtcNow.AddDays(-14),
            Rating = 5,
            Summary = "변수명 하나, 함수 길이 하나에도 책임감을 갖게 해주는 명저."
        };

        var ub2 = new UserBook
        {
            BookId = book2.Id,
            Status = ReadingStatus.Completed,
            CurrentPage = 340,
            StartDate = DateTime.UtcNow.AddMonths(-1),
            CompletedDate = DateTime.UtcNow.AddDays(-5),
            Rating = 5,
            Summary = "성장의 정체기가 올 때마다 꺼내 읽고 싶은 책."
        };

        var ub3 = new UserBook
        {
            BookId = book3.Id,
            Status = ReadingStatus.Wishlist,
            CurrentPage = 0
        };

        context.UserBooks.AddRange(ub1, ub2, ub3);
        context.SaveChanges();

        var note1 = new ReadingNote
        {
            UserBookId = ub1.Id,
            PageNumber = 42,
            Quote = "의도를 분명히 밝혀라. 좋은 이름을 지으려면 시간이 걸리지만 좋은 이름으로 절약하는 시간이 훨씬 더 많다.",
            Thought = "**네이밍의 가치**: 코드를 읽는 시간이 작성하는 시간보다 10배 이상 많다는 점을 항상 기억해야겠다. 함수나 모델 변수명을 지을 때 약어를 남발하지 말자.",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var note2 = new ReadingNote
        {
            UserBookId = ub1.Id,
            PageNumber = 112,
            Quote = "함수는 한 가지를 해야 한다. 그 한 가지를 잘 해야 한다. 그 한 가지만을 해야 한다.",
            Thought = "단일 책임 원칙(SRP)의 기본. 컨트롤러의 액션 메서드나 서비스 로직을 리팩토링할 때 기준점으로 삼자.",
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var note3 = new ReadingNote
        {
            UserBookId = ub2.Id,
            PageNumber = 88,
            Quote = "장인정신은 단순히 일을 해내는 것이 아니라, 그 일에 자부심을 느끼고 끊임없이 숙련도를 높이는 태도다.",
            Thought = "신입 개발자로서 갖추어야 할 가장 중요한 마인드셋. 코드 리뷰를 적극적으로 받고 피드백을 두려워하지 말자.",
            CreatedAt = DateTime.UtcNow.AddDays(-6)
        };

        context.ReadingNotes.AddRange(note1, note2, note3);
        context.SaveChanges();
    }
}
