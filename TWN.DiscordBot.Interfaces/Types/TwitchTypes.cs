using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWN.DiscordBot.Interfaces.Types;

#region Streams
public record StreamsResponse(StreamsResponseData[] Data, StreamsResponsePagination Pagination);

public record StreamsResponsePagination(string Cursor);

public record StreamsResponseData(
  string ID,
  string User_Id,
  string User_Login,
  string User_Name,
  string Game_ID,
  string Game_Name,
  string Type,
  string Title,
  int Viewer_Count,
  DateTime Started_At,
  string Language,
  string Thumbnail_Url,
  object[] Tag_IDs,
  string[] Tags,
  bool Is_Mature);
#endregion

#region Users
public record UsersResponse(UsersResponseData[] Data);

public record UsersResponseData(
  string ID,
  string Login,
  string Display_Name,
  string Type,
  string Broadcaster_Type,
  string Description,
  string Profile_Image_Url,
  string Offline_Image_Url,
  int View_Count,
  DateTime Created_At);

#endregion