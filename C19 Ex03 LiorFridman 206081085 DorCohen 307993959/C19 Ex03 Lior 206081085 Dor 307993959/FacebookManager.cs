﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FacebookWrapper;
using FacebookWrapper.ObjectModel;

namespace C19_Ex03_LiorFridman_206081085_DorCohen_307993959
{
	public sealed class FacebookManager
	{
		public User LoggedInUser { get; set; }

		public LoginResult LoginResult { get; set; }

		public AppSettings AppSettingsInstance { get; set; }

		private static FacebookManager m_FacebookManager = null;

		public MatchFinderFeature MatchFinder { get; set; }

		public FacebookServiceFacade FaceBookServiceFacade { get; set; }

		public FriendListSorter FriendListSorter { get; set; }

		private List<User> m_LoggedInUserFriends = new List<User>();

		public List<User> LoggedInUserFriends
		{
			get
			{
				return m_LoggedInUserFriends;
			}

			set
			{
				m_LoggedInUserFriends = value;
			}
		}

		public static FacebookManager GetInstance()
		{
			if (m_FacebookManager == null)
			{
				m_FacebookManager = new FacebookManager();
			}

			return m_FacebookManager;
		}

		private FacebookManager()
		{
			AppSettingsInstance = AppSettings.LoadFromFile();
			FaceBookServiceFacade = new FacebookServiceFacade();
		}

		private void startMatchFeature()
		{
			MatchFinder.FindMatch(LoggedInUser);
		}

		public void Login()
		{
			LoginResult = FaceBookServiceFacade.Login();
			LoggedInUser = LoginResult.LoggedInUser;
			foreach (User friend in LoggedInUser.Friends)
			{
				m_LoggedInUserFriends.Add(friend);
			}
		}

		public List<string> FetchUserPosts()
		{
			List<string> listPosts = new List<string>();
			try
			{
				foreach (Post post in LoggedInUser.Posts)
				{
					if (post.Message != null)
					{
						listPosts.Add(post.Message);
					}
					else if (post.Caption != null)
					{
						listPosts.Add(post.Caption);
					}
					else
					{
						listPosts.Add(string.Format("[{0}]", post.Type));
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			return listPosts;
		}

		public void Logout()
		{
			FaceBookServiceFacade.Logout();
			LoggedInUser = null;
			LoginResult = null;
			AppSettingsInstance.RememberUser = false;
		}

		public int FindBestTimeToUploadAPicture(ref string io_MostLikesPicture)
		{
			FacebookObjectCollection<Album> albums;
			albums = LoggedInUser.Albums;
			List<PhotosAndLikes> listOfPhotosLikeByTime = new List<PhotosAndLikes>();
			for (int i = 0; i < 24; i++)
			{
				listOfPhotosLikeByTime.Add(new PhotosAndLikes(0, 0));
			}

			foreach (Album album in albums)
			{
				foreach (Photo photo in album.Photos)
				{
					listOfPhotosLikeByTime[photo.CreatedTime.Value.Hour].NumOfPhotos += 1;
					listOfPhotosLikeByTime[photo.CreatedTime.Value.Hour].TotalLikes += photo.LikedBy.Count;
					listOfPhotosLikeByTime[photo.CreatedTime.Value.Hour].Pictures.Add(photo);
				}
			}

			PhotosAndLikes bestHourPhotoAndLikes = new PhotosAndLikes(0, 0);
			float maxLikePerPhoto = 0;
			float likesPerPhoto;
			int bestHourToPhoto = 0;
			int hour = 0;
			foreach (PhotosAndLikes photosAndLikes in listOfPhotosLikeByTime)
			{
				if (photosAndLikes.NumOfPhotos != 0)
				{
					likesPerPhoto = photosAndLikes.TotalLikes / photosAndLikes.NumOfPhotos;
				}
				else
				{
					likesPerPhoto = 0;
				}

				if (maxLikePerPhoto < likesPerPhoto)
				{
					bestHourPhotoAndLikes = photosAndLikes;
					maxLikePerPhoto = likesPerPhoto;
					bestHourToPhoto = hour;
				}

				hour += 1;
			}

			Photo bestPhotoOnBestTime = new Photo();
			foreach(Photo photo in bestHourPhotoAndLikes)
			{
				if (bestPhotoOnBestTime.LikedBy.Count < photo.LikedBy.Count)
				{
					bestPhotoOnBestTime = photo;
				}
			}

			io_MostLikesPicture = bestPhotoOnBestTime.PictureNormalURL;
			return bestHourToPhoto;
		}

		public void SendMail(string i_EmailAddress)
		{
			MailMessage mail = new MailMessage();
			SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
			mail.From = new MailAddress(@"facebookuserdorlior@gmail.com");
			mail.Subject = "Hey! Facebook sending mail feature";
			mail.To.Add(i_EmailAddress);
			mail.Body = string.Format(@"Hey, {0} has found that you are his match!! ", LoggedInUser.Name);
			smtpServer.Port = 587;
			smtpServer.Credentials = new NetworkCredential(@"facebookuserdorlior@gmail.com", "Fb123456");
			smtpServer.EnableSsl = true;
			smtpServer.Send(mail);
		}

		public void SortFriendByFirstName()
		{
			FriendListSorter = FriendListSorter.CreateSorter(eSortType.SortByFirstName);
			FriendListSorter.Sort(ref m_LoggedInUserFriends);
		}

		public void SortFriendByLastName()
		{
			FriendListSorter = FriendListSorter.CreateSorter(eSortType.SortByLastName);
			FriendListSorter.Sort(ref m_LoggedInUserFriends);
		}

		public void RunMatchByPhotos()
		{
			MatchFinder = new MatchFinderFeature(new MatcherByPhotos());
			startMatchFeature();
		}

		public void RunMatchByGroups()
		{
			MatchFinder = new MatchFinderFeature(new MatcherByGroups());
			startMatchFeature();
		}

		public void RunMatchByFriends()
		{
			MatchFinder = new MatchFinderFeature(new MatcherByFriends());
			startMatchFeature();
		}
	}
}
