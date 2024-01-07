BEGIN TRANSACTION;

update baseball_players
set
    contact = @Contact,
    power = @Power,
    mojo = @Mojo,
    speed = @Speed,
    arm = @Arm,
    fielding = @Fielding
where id = @PlayerId
and teamID = @TeamId;

update character_attributes
set
    firstName = @FirstName,
    lastName = @LastName
from character_attributes as ca join main.team_configuration_character_attribute_joins tccaj on ca.ID = tccaj.characterAttributesID
join main.baseball_players bp on tccaj.baseballPlayerID = bp.ID
where bp.id = @PlayerId
and tccaj.teamConfigurationID = @TeamConfigurationId
and character_attributes.id = ca.id;

COMMIT;

-- ROLLBACK
