﻿#if !NO_LITE_DB
using System;
using System.Collections.Generic;
using System.Linq;
using Internal.LiteDB.Engine;
using static Internal.LiteDB.Constants;

namespace Internal.LiteDB
{
    internal partial class SqlParser
    {
        /// <summary>
        /// RENAME COLLECTION {collection} TO {newName}
        /// </summary>
        private BsonDataReader ParseRename()
        {
            _tokenizer.ReadToken().Expect("RENAME");
            _tokenizer.ReadToken().Expect("COLLECTION");

            var collection = _tokenizer.ReadToken().Expect(TokenType.Word).Value;

            _tokenizer.ReadToken().Expect("TO");

            var newName = _tokenizer.ReadToken().Expect(TokenType.Word).Value;

            _tokenizer.ReadToken().Expect(TokenType.EOF, TokenType.SemiColon);

            var result = _engine.RenameCollection(collection, newName);

            return new BsonDataReader(result);
        }
    }
}
#endif