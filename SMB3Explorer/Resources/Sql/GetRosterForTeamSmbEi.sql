select
    bp.ID as playerId,
    firstName,
    lastName,
    t.name as teamName,
    bpp.abbreviatedName as primaryPosition,
    bpp2.abbreviatedName as secondaryPosition,
    bp.contact as contactRating,
    bp.power as powerRating,
    bp.mojo as mojoRating,
    bp.speed as speedRating,
    bp.arm as armRating,
    bp.fielding as fieldingRating
from baseball_players bp
join main.teams t on bp.teamID = t.ID
join main.team_configuration_character_attribute_joins tccaj on bp.ID = tccaj.baseballPlayerID
join main.character_attributes ca on tccaj.characterAttributesID = ca.ID
join baseball_player_positions bpp on bpp.ID = bp.primaryPositionID
join baseball_player_positions bpp2 on bpp2.id = bp.secondaryPositionID
where t.id = @TeamId
and tccaj.teamConfigurationID = @TeamConfigurationId

