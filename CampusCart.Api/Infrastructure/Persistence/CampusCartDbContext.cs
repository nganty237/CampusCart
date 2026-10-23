using CampusCart.Api.Modules.Auth;
using CampusCart.Api.Modules.Listings;
using CampusCart.Api.Modules.Messaging;
using CampusCart.Api.Modules.Requests;
using CampusCart.Api.Modules.Reviews;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusCart.Api.Infrastructure.Persistence;

public sealed class CampusCartDbContext(
    DbContextOptions<CampusCartDbContext> options)
    : IdentityDbContext<CampusCartUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<ServiceListing> Listings => Set<ServiceListing>();
    public DbSet<ServiceRequest> Requests => Set<ServiceRequest>();
    public DbSet<ServiceReview> Reviews => Set<ServiceReview>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<EmailVerificationChallenge> EmailVerificationChallenges => Set<EmailVerificationChallenge>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CampusCartUser>(entity =>
        {
            entity.Property(user => user.FullName).HasMaxLength(120).IsRequired();
        });

        builder.Entity<EmailVerificationChallenge>(entity =>
        {
            entity.ToTable("email_verification_challenges");
            entity.HasKey(challenge => challenge.Id);
            entity.Property(challenge => challenge.CodeHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(challenge => new { challenge.UserId, challenge.CreatedAtUtc });
            entity.HasOne<CampusCartUser>()
                .WithMany()
                .HasForeignKey(challenge => challenge.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ServiceListing>(entity =>
        {
            entity.ToTable("service_listings");
            entity.HasKey(listing => listing.Id);
            entity.Property(listing => listing.Title).HasMaxLength(120).IsRequired();
            entity.Property(listing => listing.Category).HasMaxLength(60).IsRequired();
            entity.Property(listing => listing.Description).HasMaxLength(2000).IsRequired();
            entity.Property(listing => listing.Unit).HasMaxLength(80).IsRequired();
            entity.Property(listing => listing.Location).HasMaxLength(120).IsRequired();
            entity.Property(listing => listing.PriceXaf).HasColumnName("price_xaf");
            entity.Property(listing => listing.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(listing => new { listing.IsActive, listing.CreatedAtUtc });
            entity.HasOne(listing => listing.Provider)
                .WithMany()
                .HasForeignKey(listing => listing.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint(
                "ck_service_listings_price_xaf_positive",
                "\"price_xaf\" > 0"));
        });

        builder.Entity<ServiceRequest>(entity =>
        {
            entity.ToTable("service_requests");
            entity.HasKey(request => request.Id);
            entity.Property(request => request.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(request => request.HandoverLocation).HasMaxLength(160).IsRequired();
            entity.Property(request => request.Message).HasMaxLength(1000).IsRequired();
            entity.Property(request => request.PriceXaf).HasColumnName("price_xaf");
            entity.Property(request => request.ConcurrencyToken).IsConcurrencyToken();
            entity.HasIndex(request => new { request.RequesterId, request.CreatedAtUtc });
            entity.HasIndex(request => new { request.ProviderId, request.CreatedAtUtc });
            entity.HasOne(request => request.Listing)
                .WithMany()
                .HasForeignKey(request => request.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CampusCartUser>()
                .WithMany()
                .HasForeignKey(request => request.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CampusCartUser>()
                .WithMany()
                .HasForeignKey(request => request.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint(
                "ck_service_requests_price_xaf_positive",
                "\"price_xaf\" > 0"));
        });

        builder.Entity<ServiceReview>(entity =>
        {
            entity.ToTable("service_reviews");
            entity.HasKey(review => review.Id);
            entity.Property(review => review.Rating).HasColumnName("rating");
            entity.Property(review => review.Comment).HasMaxLength(1000);
            entity.HasIndex(review => review.RequestId).IsUnique();
            entity.HasOne(review => review.Request)
                .WithMany()
                .HasForeignKey(review => review.RequestId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CampusCartUser>()
                .WithMany()
                .HasForeignKey(review => review.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CampusCartUser>()
                .WithMany()
                .HasForeignKey(review => review.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint(
                "ck_service_reviews_rating_range",
                "\"rating\" >= 1 AND \"rating\" <= 5"));
        });

        builder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(conversation => conversation.Id);
            entity.HasIndex(conversation => conversation.RequestId).IsUnique();
            entity.HasOne(conversation => conversation.Request)
                .WithOne(request => request.Conversation)
                .HasForeignKey<Conversation>(conversation => conversation.RequestId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ConversationMessage>(entity =>
        {
            entity.ToTable("conversation_messages");
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Body).HasMaxLength(2000).IsRequired();
            entity.HasIndex(message => new { message.ConversationId, message.CreatedAtUtc });
            entity.HasOne(message => message.Conversation)
                .WithMany(conversation => conversation.Messages)
                .HasForeignKey(message => message.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<CampusCartUser>()
                .WithMany()
                .HasForeignKey(message => message.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
