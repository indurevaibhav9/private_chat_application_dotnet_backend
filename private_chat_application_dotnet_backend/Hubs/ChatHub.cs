namespace private_chat_application_dotnet_backend.Hubs
{
    using Microsoft.AspNetCore.SignalR;

    public class ChatHub : Hub
    {
        // Called when user logs in — joins group = userId
        public async Task RegisterUser(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task StartCall(string callerId, string receiverId)
        {
            await Clients.Group(receiverId).SendAsync("IncomingCall", callerId);
        }

        // Receiver accepts the call
        public async Task AcceptCall(string callerId, string receiverId)
        {
            await Clients.Group(callerId).SendAsync("CallAccepted", receiverId);
        }

        // Receiver rejects the call
        public async Task RejectCall(string callerId, string receiverId)
        {
            await Clients.Group(callerId).SendAsync("CallRejected", receiverId);
        }

        // WebRTC: send SDP offer/answer
        public async Task SendSdp(string targetUserId, string sdp)
        {
            await Clients.Group(targetUserId).SendAsync("ReceiveSdp", sdp);
        }

        // WebRTC: send ICE candidates
        public async Task SendIceCandidate(string targetUserId, string candidate)
        {
            await Clients.Group(targetUserId).SendAsync("ReceiveIceCandidate", candidate);
        }

        public async Task EndCall(string userId, string targetUserId)
        {
            // Notify the other user that call has ended
            await Clients.Group(targetUserId).SendAsync("CallEnded", userId);
        }

    }
}
