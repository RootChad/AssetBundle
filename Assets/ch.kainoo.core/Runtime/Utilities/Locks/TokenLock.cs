using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.core
{

    public class TokenLock
    {
        public bool IsLocked => _tokens.Count > 0;

        private HashSet<Token> _tokens = new HashSet<Token>();


        public Token GetToken()
        {
            var token = new Token(this);
            _tokens.Add(token);
            return token;
        }

        public bool Release(Token token)
        {
            bool isRemoved = _tokens.Remove(token);
            if (isRemoved)
            {
                token.IsReleased = true;
            }
            return isRemoved;
        }



        public class Token
        {
            public bool IsReleased { get; internal set; } = false;

            private TokenLock _tokenLock;


            internal Token(TokenLock tokenLock)
            {
                _tokenLock = tokenLock;
            }

            ~Token()
            {
                _tokenLock?.Release(this);
            }


            /// <summary>
            /// Easily release the token on the lock. 
            /// <para/>
            /// This is equivalent to: 
            /// <code>
            /// TokenLock tokenLock = ...;
            /// TokenLock.Token token = tokenLock.Lock();
            /// tokenLock.Release(token);
            /// </code>
            /// </summary>
            public void Release()
            {
                _tokenLock.Release(this);
            }

        }

    }

}