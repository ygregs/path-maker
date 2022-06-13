using System;
using Unity.Services.Lobbies;

namespace PathMaker.lobby
{
    public class AsyncRequestLobby : AsyncRequest
    {
        private static AsyncRequestLobby s_instance;
        public static AsyncRequestLobby Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new AsyncRequestLobby();
                }
                return s_instance;
            }
        }

        protected override void ParseServiceException(Exception e)
        {
            if (!(e is LobbyServiceException))
            {
                return;
            }
            var lobbyEx = e as LobbyServiceException;
            if (lobbyEx.Reason == LobbyExceptionReason.RateLimited)
            { return; }
            Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, $"Lobby Error: {lobbyEx.Message} ({lobbyEx.InnerException.Message})");
        }
    }
}