SELECT t_baseball_players.GUID     as guid,
       attrFName.optionValue       as firstName,
       attrLName.optionValue       as lastName,
       attrPrimaryPos.optionValue  as primaryPosition,
       t_baseball_players.power    as powerRating,
       t_baseball_players.contact  as contactRating,
       t_baseball_players.speed    as speedRating,
       t_baseball_players.arm      as armRating,
       t_baseball_players.fielding as fieldingRating,
       t_baseball_players.velocity as velocityRating,
       t_baseball_players.accuracy as accuracyRating,
       t_baseball_players.junk     as junkRating
FROM t_baseball_player_local_ids
         JOIN t_baseball_player_options attrFName
              ON attrFName.[baseballPlayerLocalID] = t_baseball_player_local_ids.[localID] AND
                 attrFName.[optionKey] = 66
         JOIN t_baseball_player_options attrLName
              ON attrLName.[baseballPlayerLocalID] = t_baseball_player_local_ids.[localID] AND
                 attrLName.[optionKey] = 67
         JOIN t_baseball_player_options attrPrimaryPos
              ON attrPrimaryPos.[baseballPlayerLocalID] = t_baseball_player_local_ids.[localID] AND
                 attrPrimaryPos.[optionKey] = 54
         LEFT JOIN t_baseball_player_options attrPitcherRole
                   ON attrPitcherRole.[baseballPlayerLocalID] = t_baseball_player_local_ids.[localID] AND
                      attrPitcherRole.[optionKey] = 57
         JOIN t_baseball_players on t_baseball_player_local_ids.GUID = t_baseball_players.GUID
where teamGUID = @TeamGUID;
