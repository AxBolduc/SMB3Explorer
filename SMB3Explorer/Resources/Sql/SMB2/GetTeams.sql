select GUID, teamName, isBuiltIn, teamType from t_teams
where templateTeamFamily is null and originalGUID is null
