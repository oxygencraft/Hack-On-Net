using HackLinks_Server.Computers;
using System;

namespace HackLinks_Server.Daemons.Types.Mail {
    class PassResetRequest {
        private static Random _random = new Random();

        private int _authCode = _random.Next(100000, 999999);

        private DateTime _timeRequested;

        private TimeSpan _timeExpire;

        private string _username;

        private Node _accountServer;

        private bool _isValid = true;

        /// <summary>
        /// Creates a new password reset request
        /// </summary>
        /// <param name="node">The node of the server that the account is on</param>
        /// <param name="username">The username of the account</param>
        public PassResetRequest(Node node, string username, out int authCode) {
            authCode = _authCode;
            _accountServer = node;
            _username = username;
            _timeRequested = DateTime.UtcNow;
            _timeExpire = TimeSpan.FromMinutes(60);
        }

        /// <summary>
        /// Checks if the authentification request is valid
        /// </summary>
        /// <param name="node">The node the account is on</param>
        /// <param name="username">The username of the account</param>
        /// <param name="authcode">The authentication code</param>
        /// <returns></returns>
        public bool CheckAuthRequest(Node node, string username, int authcode) {
            if (node == _accountServer && username == _username && authcode == _authCode)
                return true;
            return false;
        }

        public bool CheckTime() {
            if (DateTime.UtcNow - _timeRequested < _timeExpire)
                return true;
            return false;
        }

        public Node GetNode() { return _accountServer; }

        public string GetUsername() { return _username; }
    }
}
