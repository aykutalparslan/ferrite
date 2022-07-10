// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using Ferrite.Data;
using Ferrite.TL.currentLayer;

namespace Ferrite.TL.ObjectMapper;

public class UpdateMapper : ITLObjectMapper<Update, UpdateBase>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public UpdateMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public UpdateBase MapToDTO(Update obj)
    {
        throw new NotImplementedException();
    }

    public Update MapToTLObject(UpdateBase obj)
    {
        if (obj is UpdateMessageIdDTO updateMessageId)
        {
            var update = _factory.Resolve<UpdateMessageIDImpl>();
            update.Id = updateMessageId.Id;
            update.RandomId = updateMessageId.RandomId;
            return update;
        }
        throw new NotSupportedException("Update type is not supported");
    }
}