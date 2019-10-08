using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.SessionHandlers;
using InstagramApiSharp.Logger;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace InstagramBot_03
{
    public partial class Form1 : Form
    {
       
        public class User :Form1
        {

            const string AppName = "Challenge Required";
            const string StateFile = "state.bin";
            readonly Size NormalSize = new Size(432, 164);
            readonly Size ChallengeSize = new Size(432, 604);
            private static IInstaApi instaApi;
            UserSessionData userSession;
            bool isEmail = false;
            RichTextBox t;
            IResult<InstaDirectInboxContainer> inboxThreads;


            public User(RichTextBox t)
            {
                this.t = t;
            }

            public void InitializeUserSessionData(String username, String password)
            {
                if(String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                {
                    username = "buritimka1";
                    password = "Justfuture";
                }
                userSession = new UserSessionData
                {
                    UserName = username,
                    Password = password,
                };
                
            }

            public void InitializeInstaApi()
            {
                if (userSession != null)
                {
                    instaApi = InstaApiBuilder.CreateBuilder()
                   .SetUser(userSession)
                   .UseLogger(new DebugLogger(LogLevel.All))
                   .SetRequestDelay(RequestDelay.FromSeconds(0, 1))
                   // Session handler, set a file path to save/load your state/session data
                   .SetSessionHandler(new FileSessionHandler() { FilePath = StateFile })
                   .Build();
                    LoadSession();
                }
            }

            public async void LoginUser()
            {
                Log("***Login***User***");
                if (/*!instaApi.IsUserAuthenticated*/true )
                {
                    Log("User is not Authentificated");
                    var logInResult = await instaApi.LoginAsync();
                    if (logInResult.Succeeded)
                    {
                        Log("Login result is SUCCESS");
                        // Save session 
                        SaveSession();
                    }
                    else
                    {
                        Log("Login result is FAIL");
                        if (logInResult.Value == InstaLoginResult.ChallengeRequired)
                        {
                            Log("Need verify Require challenge");

                            var challenge = await instaApi.GetChallengeRequireVerifyMethodAsync();
                            if (challenge.Succeeded)
                            {
                                Log("Challenge start!");
                                if (challenge.Value.SubmitPhoneRequired)
                                {
                                    Log("Submit Phone");

                                }
                                else
                                {
                                    Log("Not submit phone");
                                    if (challenge.Value.StepData != null)
                                    {
                                        Log("Try get phone or email adress");
                                        if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
                                        {
                                            Log("Phone adress!");
                                            Adress.Text = challenge.Value.StepData.PhoneNumber;
                                        }
                                        if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
                                        {
                                            isEmail = true;
                                            Log("Email adress!");                                         
                                            Adress.Text = challenge.Value.StepData.Email;
                                        }                                                                               
                                    }
                                }
                            }
                            else
                            {
                                Log("Challenge ERROR");
                            }
                        }
                        else if (logInResult.Value == InstaLoginResult.TwoFactorRequired)
                        {
                            Log("2 factor required");
                        }
                    }
                }
                else
                {
                    Log("You are already Authentificated");
                }
            }


            public async void SendVerificationCode()
            {
                Log("***Send***Code***");
                try
                {
                    // Note: every request to this endpoint is limited to 60 seconds                 
                    if (isEmail)
                    {
                        Log("Email request to get code");
                        // send verification code to email
                        var email = await instaApi.RequestVerifyCodeToEmailForChallengeRequireAsync();
                        if (email.Succeeded)
                        {
                            Log("Email request is succeeded");
                            Log("We send code to this Email: " + email.Value.StepData.ContactPoint);
                            Adress.Text = "We send code to this Email: " + email.Value.StepData.ContactPoint;

                        }
                        else
                            Log("Fail email request to get code");
                    }
                    else
                    {
                        Log("Number request to get code");
                        // send verification code to phone number
                        var phoneNumber = await instaApi.RequestVerifyCodeToSMSForChallengeRequireAsync();
                        if (phoneNumber.Succeeded)
                        {
                            Log("Number request is succeeded");
                            Log("We send code to this number: " + phoneNumber.Value.StepData.ContactPoint);
                            Adress.Text = "We send code to this number: " + phoneNumber.Value.StepData.ContactPoint;
                        }
                        else
                            Log("Fail phone number request to get code");
                    }
                }
                catch (Exception ex) { Log("Catch Error!"); }

            }

            public async void VerifyCode(String code)
            {
                Log("***Verify***Code***");
                try
                {
                    // Note: calling VerifyCodeForChallengeRequireAsync function, 
                    // if user has two factor enabled, will wait 15 seconds and it will try to
                    // call LoginAsync.
                    Log("Verify code try");
                    var verifyLogin = await instaApi.VerifyCodeForChallengeRequireAsync(code);
                    if (verifyLogin.Succeeded)
                    {
                        Log("Verification code try SUCCESS");
                        Log("CONNECTION SUCCESS");
                        // you are logged in sucessfully.
                       
                        // Save session
                        SaveSession();
                        
                    }
                    else
                    {
                        Log("Verify code try fail");

                        // two factor is required
                        if (verifyLogin.Value == InstaLoginResult.TwoFactorRequired)
                        {
                            Log("It is 2 factor Auth");

                        }
                        else
                            Log("VERIFICATION FAIL");
                    }

                }
                catch (Exception ex) { Log("CATCH ERROR"); }

            }

           public async void SendMessageViaUsername(String str)
            {
                Log("***Inbox***&***Send***");
             
                var desireUsername = "smileresearcher";
                var user = await instaApi.UserProcessor.GetUserAsync(desireUsername);
                var userId = user.Value.Pk.ToString();
                var directText = await instaApi.MessagingProcessor
                    .SendDirectTextAsync(userId, null, str);
            }

            public async void GetAllRequests()
            {
                // inboxThreads = await instaApi.MessagingProcessor.GetDirectInboxAsync(InstagramApiSharp.PaginationParameters.MaxPagesToLoad(1));
                GetLastMessages();
            }

            public async void GetLastMessages()
            {
                Log("***GET***LAST***MESSAGES***");
                inboxThreads = await instaApi.MessagingProcessor.GetDirectInboxAsync(InstagramApiSharp.PaginationParameters.MaxPagesToLoad(1));                               
                var threads = inboxThreads.Value.Inbox.Threads;
                var countThreads = Math.Min(threads.Count, Constants.COUNT_THREADS_TO_CHECK);              
                List<ThreadInbox> Dialogs = new List<ThreadInbox>();
                for (int i = 0; i < countThreads; i++){
                    var threadInbox = await instaApi.MessagingProcessor.GetDirectInboxThreadAsync(threads[i].ThreadId, InstagramApiSharp.PaginationParameters.MaxPagesToLoad(1));
                    var threadId = threads[i].ThreadId;
                    var mes = threadInbox.Value.Items;
                    ThreadInbox thInbox = new ThreadInbox(threadId);
                    thInbox.UserName = threads[i].Inviter.UserName;
                    thInbox.AddMessages(mes);
                    Dialogs.Add(thInbox);
                }
                foreach(var dialog in Dialogs)
                {
                    Log("***" + dialog.UserName + "***");
                    foreach(var mes in dialog.messages)
                    {
                        Log("\t" + mes.user_id+" : "+mes.text);
                    }
                }                
            }



            



            void SaveSession()
            {
                if (instaApi == null)
                    return;
                if (!instaApi.IsUserAuthenticated)
                    return;
                instaApi.SessionHandler.Save();

                //// Old save session 
                //var state = InstaApi.GetStateDataAsStream();
                //using (var fileStream = File.Create(StateFile))
                //{
                //    state.Seek(0, SeekOrigin.Begin);
                //    state.CopyTo(fileStream);
                //}
            }

            void LoadSession()
            {
                instaApi?.SessionHandler?.Load();

                //// Old load session
                //try
                //{
                //    if (File.Exists(StateFile))
                //    {
                //        Debug.WriteLine("Loading state from file");
                //        using (var fs = File.OpenRead(StateFile))
                //        {
                //            InstaApi.LoadStateDataFromStream(fs);
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Debug.WriteLine(ex);
                //}
            }

           public void Log(String s)
            {
               // t.AppendText(s+'\n');
                t.Text = t.Text + s + '\n';
            }

        }
        /*public void Log(String s)
        {
            info.AppendText(s);
           // info.Text = info.Text + s + '\n';
        }*/
    }
}
