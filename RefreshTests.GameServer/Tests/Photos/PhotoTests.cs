using Refresh.Database.Helpers;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Photos;

public class PhotoTests : GameServerTest
{
    private const string TEST_IMAGE_HASH = "0ec63b140374ba704a58fa0c743cb357683313dd";

    [Test]
    public void CreateAndGetPhotoWithSubjects()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameUser player2 = context.CreateUser();
        GamePhoto createdPhoto = context.CreatePhotoWithSubject(publisher, TEST_IMAGE_HASH, subjects:
        [
            new()
            {
                Username = publisher.Username,
                DisplayName = publisher.Username,
                BoundsList = "1,2,3,4",
            },
            new()
            {
                Username = player2.Username,
                DisplayName = player2.Username,
                BoundsList = "4,5,6,7",
            }
        ]);

        List<GamePhotoSubject> separateSubjects = context.Database.GetSubjectsInPhoto(createdPhoto).ToList();
        GamePhoto? fetchedPhoto = context.Database.GetRecentPhotos(10, 0).Items.FirstOrDefault();
        Assert.That(fetchedPhoto, Is.Not.Null);
        Assert.That(fetchedPhoto!.PhotoId, Is.EqualTo(createdPhoto.PhotoId));

        // these 3 subject lists are fetched/created in different ways, so ensure they all contain the correct data
        Assert.That(separateSubjects.Count, Is.EqualTo(2));
        Assert.That(createdPhoto.Subjects.Count, Is.EqualTo(2));
        Assert.That(fetchedPhoto.Subjects.Count, Is.EqualTo(2));

        // Assert first subject on all lists
        Assert.That(separateSubjects[0].PlayerId, Is.EqualTo(1));
        Assert.That(createdPhoto.Subjects[0].PlayerId, Is.EqualTo(1));
        Assert.That(fetchedPhoto.Subjects[0].PlayerId, Is.EqualTo(1));

        Assert.That(separateSubjects[0].DisplayName, Is.EqualTo(publisher.Username));
        Assert.That(createdPhoto.Subjects[0].DisplayName, Is.EqualTo(publisher.Username));
        Assert.That(fetchedPhoto.Subjects[0].DisplayName, Is.EqualTo(publisher.Username));

        Assert.That(separateSubjects[0].User, Is.Not.Null);
        Assert.That(createdPhoto.Subjects[0].User, Is.Not.Null);
        Assert.That(fetchedPhoto.Subjects[0].User, Is.Not.Null);

        Assert.That(separateSubjects[0].User!.UserId, Is.EqualTo(publisher.UserId));
        Assert.That(createdPhoto.Subjects[0].User!.UserId, Is.EqualTo(publisher.UserId));
        Assert.That(fetchedPhoto.Subjects[0].User!.UserId, Is.EqualTo(publisher.UserId));

        float[] subject1Bounds = PhotoHelper.ParseBoundsList("1,2,3,4");
        Assert.That(separateSubjects[0].Bounds, Is.EqualTo(subject1Bounds));
        Assert.That(createdPhoto.Subjects[0].Bounds, Is.EqualTo(subject1Bounds));
        Assert.That(fetchedPhoto.Subjects[0].Bounds, Is.EqualTo(subject1Bounds));

        // Assert second subject on all lists
        Assert.That(separateSubjects[1].PlayerId, Is.EqualTo(2));
        Assert.That(createdPhoto.Subjects[1].PlayerId, Is.EqualTo(2));
        Assert.That(fetchedPhoto.Subjects[1].PlayerId, Is.EqualTo(2));

        Assert.That(separateSubjects[1].DisplayName, Is.EqualTo(player2.Username));
        Assert.That(createdPhoto.Subjects[1].DisplayName, Is.EqualTo(player2.Username));
        Assert.That(fetchedPhoto.Subjects[1].DisplayName, Is.EqualTo(player2.Username));

        Assert.That(separateSubjects[1].User, Is.Not.Null);
        Assert.That(createdPhoto.Subjects[1].User, Is.Not.Null);
        Assert.That(fetchedPhoto.Subjects[1].User, Is.Not.Null);

        Assert.That(separateSubjects[1].User!.UserId, Is.EqualTo(player2.UserId));
        Assert.That(createdPhoto.Subjects[1].User!.UserId, Is.EqualTo(player2.UserId));
        Assert.That(fetchedPhoto.Subjects[1].User!.UserId, Is.EqualTo(player2.UserId));

        float[] subject2Bounds = PhotoHelper.ParseBoundsList("4,5,6,7");
        Assert.That(separateSubjects[1].Bounds, Is.EqualTo(subject2Bounds));
        Assert.That(createdPhoto.Subjects[1].Bounds, Is.EqualTo(subject2Bounds));
        Assert.That(fetchedPhoto.Subjects[1].Bounds, Is.EqualTo(subject2Bounds));
    }

    [Test]
    public void DeletePhotoAndSubjectsWhenDeletingUser()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameUser subject = context.CreateUser();
        GamePhoto photo = context.CreatePhotoWithSubject(publisher, TEST_IMAGE_HASH, subjects:
        [
            new()
            {
                Username = publisher.Username,
                DisplayName = publisher.Username,
                BoundsList = "1,2,3,4",
            },
            new()
            {
                Username = subject.Username,
                DisplayName = subject.Username,
                BoundsList = "4,5,6,7",
            }
        ]);

        // Initial subjects checks
        List<GamePhotoSubject> subjects = context.Database.GetSubjectsInPhoto(photo).ToList();
        Assert.That(subjects.Count, Is.EqualTo(2));
        Assert.That(photo.Subjects.Count, Is.EqualTo(2));

        // Delete publisher and re-check
        context.Database.DeleteUser(publisher);
        context.Database.Refresh();

        // GetSubjectsInPhoto only compares the photo IDs, so if the subjects weren't cascade-deleted, they should still be found
        subjects = context.Database.GetSubjectsInPhoto(photo).ToList();
        Assert.That(subjects.Count, Is.Zero);
    }
}