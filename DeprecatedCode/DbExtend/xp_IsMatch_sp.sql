--( An extended stored procedure

if object_id('master..xp_IsMatch') is not null begin
	print 'removing xp_IsMatch'
	EXEC master..sp_dropextendedproc 'xp_IsMatch'
end
go
print 'adding xp_IsMatch'
EXEC master..sp_addextendedproc 'xp_IsMatch', 'FwSqlExtend.dll'
go
