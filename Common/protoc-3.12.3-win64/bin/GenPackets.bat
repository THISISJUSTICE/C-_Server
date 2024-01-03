protoc -I=./ --csharp_out=./ ./Protocol.proto
IF ERRORLEVEL 1 PAUSE

START ../../../PacketGenerator/bin/PacketGenerator.exe ./Protocol.proto
XCOPY /Y Protocol.cs "../../../../MMOProject/Assets/Scripts/Packet"
XCOPY /Y Protocol.cs "../../../Server/Packet"
XCOPY /Y ClientPacketManager.cs "../../../../MMOProject/Assets/Scripts/Packet"
XCOPY /Y ClientPacketManager.cs "../../../Server/Packet"