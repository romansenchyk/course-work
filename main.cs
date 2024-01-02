using System;
using System.Collections.Generic; using System.Linq;
using System.Text;

namespace TankArena.TankBot
{
public class CommandBot : IBot
{
public int ID
{
get; set;
}

public List<Enemy> Attackers
{
get; set;
}

public List<Enemy> Targets
{
get; set;
}

public Position Location
{
get; set;
}

public Position TargetPosition
{
get; set;
}

public Direction Direction
{
get; set;
}

public int Ammo
{
get; set;
}

public int Hit
{
get; set;
}

public bool WasScaning
{
get; set;
}

public Position PreviosLocation
{
get; set;
}

#region IBot Members

public void Initialize(StartGameInfo startGameInfo)
 

{
if (!Comandos.GetComandos().HasStartInfo) Comandos.GetComandos().SetStartInfo(startGameInfo);
Location = startGameInfo.StartPosition; PreviosLocation = Location;
Direction = startGameInfo.StartDirection; Ammo = startGameInfo.AmmoCount;
Hit = startGameInfo.HitPoints;
ID = Comandos.GetComandos().GetID(); Comandos.GetComandos().Add(this);
}

public OutGameInfo ProcessMove(InGameInfo inGameInfo)
{
Hit = inGameInfo.HitPoints; Attackers = inGameInfo.Attackers; Targets = inGameInfo.Targets;
Comandos.GetComandos().HasStartInfo = false; return Comandos.GetComandos().GetMoveForBot(ID);
}

public string GetName()
{
return "Jet Killer";
}

#endregion

public BotMove GetNextMove(Position targetPos)
{
BotMove nextMove = BotMove.MoveAhead;

if (Location.X < targetPos.X)
{
switch (Direction)
{
case Direction.Down:
nextMove = BotMove.TurnLeft; Direction = Direction.Right; break;
case Direction.Up:
nextMove = BotMove.TurnRight; Direction = Direction.Right; break;
case Direction.Right:
nextMove = BotMove.MoveAhead; Direction = Direction.Right; break;
case Direction.Left:
nextMove = BotMove.TurnRight; Direction = Direction.Up; break;
}
}
if (Location.X > targetPos.X)
{
switch (Direction)
{
case Direction.Down:
nextMove = BotMove.TurnRight; Direction = Direction.Left; break;
case Direction.Up:
nextMove = BotMove.TurnLeft; Direction = Direction.Left; break;
case Direction.Right:
nextMove = BotMove.TurnLeft; Direction = Direction.Up; break;
case Direction.Left:
nextMove = BotMove.MoveAhead; Direction = Direction.Left; break;
}
}

if (Location.X == targetPos.X)
{
 

if (Location.Y < targetPos.Y)
{
switch (Direction)
{
case Direction.Down:
nextMove = BotMove.MoveAhead; Direction = Direction.Down; break;
case Direction.Up:
nextMove = BotMove.TurnLeft; Direction = Direction.Left; break;
case Direction.Right:
nextMove = BotMove.TurnRight; Direction = Direction.Down; break;
case Direction.Left:
nextMove = BotMove.TurnLeft; Direction = Direction.Down; break;
}
}
if (Location.Y > targetPos.Y)
{
switch (Direction)
{
case Direction.Down:
nextMove = BotMove.TurnRight; Direction = Direction.Left; break;
case Direction.Up:
nextMove = BotMove.MoveAhead; Direction = Direction.Up; break;
case Direction.Right:
nextMove = BotMove.TurnLeft; Direction = Direction.Up; break;
case Direction.Left:
nextMove = BotMove.TurnRight; Direction = Direction.Up; break;
}
}
}

if (nextMove == BotMove.MoveAhead)
{
PreviosLocation = new Position() { X = Location.X, Y = Location.Y }; switch (Direction)
{
case Direction.Down: Location.Y++; break;
case Direction.Up: Location.Y--; break;
case Direction.Right: Location.X++; break;
case Direction.Left: Location.X--; break;
}
}

return nextMove;
}

}

public class Comandos
{
SortedList<int, CommandBot> _comandosList; SortedList<int, Enemy> _enemies;
static Comandos _Comandos; CommandBot _currentBot;

int _ArenaWidth;
 

int _ArenaHeight; List<Position> _hitPoints; List<Position> _ammoPoints; int _AttrackRange;
int _ScanRange; int _SplashRange;
int _VisibilityRange; int _MaxAmmor;
int _MaxHP;

int CRITICAL_AMMO; int CRITICAL_HP; int MAX_FREE_MOVES;

int _FreeMoves; int _ActivBotID;
bool _onlyMyCommand;

public bool HasStartInfo
{
get; set;
}

private Comandos()
{
_comandosList = new SortedList<int, CommandBot>();
_enemies = new SortedList<int, Enemy>(); HasStartInfo = false;
}

public static Comandos GetComandos()
{
if (_Comandos == null)
{
_Comandos = new Comandos();
}

return _Comandos;
}

public void Add(CommandBot bot)
{
_comandosList.Add(bot.ID, bot);
}

public int GetID()
{
return _comandosList.Count;
}

public void SetStartInfo(StartGameInfo startGameInfo)
{
_ArenaWidth = startGameInfo.ArenaWidth;
_ArenaHeight = startGameInfo.ArenaHeight;
_ammoPoints = startGameInfo.AmmoPoints;
_hitPoints = startGameInfo.HPPoints;
_AttrackRange = startGameInfo.AttrackRange;
_ScanRange = startGameInfo.ScanRange;
_SplashRange = startGameInfo.SplashRange;
_VisibilityRange = startGameInfo.VisibilityRange;
_MaxAmmor = startGameInfo.AmmoCount;
_MaxHP = startGameInfo.HitPoints;

MAX_FREE_MOVES = (startGameInfo.ArenaWidth + startGameInfo.ArenaHeight) / 2;
_FreeMoves = MAX_FREE_MOVES;
_ActivBotID = -1;
_onlyMyCommand = false;

CRITICAL_AMMO = 4;
CRITICAL_HP = 90;

_comandosList.Clear();
_enemies.Clear();

HasStartInfo = true;
}

public OutGameInfo GetMoveForBot(int id)
 

{
OutGameInfo outInfo = new OutGameInfo(); Enemy enemy = null;

if (_comandosList[id] != null)
{
_currentBot = _comandosList[id]; RemoveDeadTank(); UpdateBotEnemyList(_currentBot.Attackers); UpdateBotEnemyList(_currentBot.Targets);

if (_currentBot.WasScaning)
_currentBot.WasScaning = false;

if (_onlyMyCommand && id != _ActivBotID)
{
outInfo.Move = BotMove.Skip; return outInfo;
}

if (((_currentBot.Hit > CRITICAL_HP && _currentBot.Ammo > CRITICAL_AMMO)
|| _currentBot.Targets.Count > 0) && _currentBot.Ammo > 0)
{
if (IsWeOnDeadPoint())
{
 

= _currentBot.Location.Y };
 
Position nextPos = new Position() { X = _currentBot.Location.X, Y

switch (_currentBot.Direction)
{
 
case Direction.Down: nextPos.Y++; break;
case Direction.Up: nextPos.Y--; break;
case Direction.Right: nextPos.X++; break;
case Direction.Left: nextPos.X--; break;
}
outInfo.Move = GetNextMove(nextPos); return outInfo;
}

enemy = FindBestEnemyInAttrackRange(); if (enemy != null)
{
outInfo.Move = BotMove.Shoot; outInfo.TargetPosition = enemy.Position; enemy.HitPoints -= 20;
_currentBot.Ammo--; return outInfo;
}
else
{
enemy = FindBestEnemy(); if (enemy != null)
{
 

_AttrackRange)
 
if	(GetDistance(_currentBot.Location,	enemy.Position)	<=

{
outInfo.Move = BotMove.Scan;
_currentBot.WasScaning = true;
 
}
else
 

outInfo.Move = GetNextMove(enemy.Position);
 
return outInfo;
}
else
{
if	((_currentBot.TargetPosition	==	null	||
_currentBot.TargetPosition.Equals(_currentBot.Location)))
{
_currentBot.TargetPosition = GetRandPos(); outInfo.Move = BotMove.Scan;
_currentBot.WasScaning = true;
 

 
}
else
 


outInfo.Move = GetNextMove(_currentBot.TargetPosition);
 

}
}
}
else
{
 
return outInfo;
 
if (_currentBot.Hit <= CRITICAL_HP)
{
Position p = GetMinPath(_hitPoints);

outInfo.Move = GetNextMove(p); return outInfo;
}

if (_currentBot.Ammo <= CRITICAL_AMMO)
{
Position p = GetMinPath(_ammoPoints);

if (p.Equals(_currentBot.Location))
_currentBot.Ammo = _MaxAmmor;

outInfo.Move = GetNextMove(p); return outInfo;
}
}
}

return outInfo;
}

private BotMove GetNextMove(Position pos)
{
BotMove move = _currentBot.GetNextMove(pos);

return move;
}

private bool IsWeOnDeadPoint()
{
if (_currentBot.Location.Equals(_ammoPoints[0]) ||
_currentBot.Location.Equals(_ammoPoints[1]) ||
_currentBot.Location.Equals(_hitPoints[0]) ||
_currentBot.Location.Equals(_hitPoints[1])) return true;
return false;
}

private void UpdateBotEnemyList(List<Enemy> enemys)
{
if (enemys != null && enemys.Count > 0)
{
foreach (Enemy en in enemys)
{
if (!_enemies.ContainsKey(en.ID))
{
AddEnemy(en);
}
else
{
_enemies[en.ID].HitPoints = en.HitPoints;
_enemies[en.ID].Position = en.Position;
}
}
}

RemoveDeadEnemy();

if (_enemies.Count == 0 && _FreeMoves > 0)
{
_FreeMoves--;
}

if (_FreeMoves <= 0 && _enemies.Count == 0)
{
_onlyMyCommand = true;
_ActivBotID = _currentBot.ID;
 

CRITICAL_AMMO = 0;
}
}

private void AddEnemy(Enemy en)
{
if (!IsMyTank(en))
{
if (_onlyMyCommand)
{
_FreeMoves = MAX_FREE_MOVES; CRITICAL_AMMO = 4;
_ActivBotID = -1;
_enemies.Clear();
_onlyMyCommand = false;
}
_enemies.Add(en.ID, en);
}

if (_onlyMyCommand && en.ID != _ActivBotID)
_enemies.Add(en.ID, en);
}

private void RemoveDeadEnemy()
{
for (int i = 0; i < _enemies.Count; i++)
{
if (_currentBot.WasScaning)
{
 

_ScanRange
 
if (GetDistance(_currentBot.Location, _enemies.Values[i].Position) <=

&& !IsEnemyExistIn(_enemies.Values[i], _currentBot.Targets))
{
_enemies.RemoveAt(i); i--;
continue;
 
}
}
if (_enemies.Values[i].HitPoints <= 0)
{
_enemies.RemoveAt(i); i--;
}
}
}

private void RemoveDeadTank()
{

for (int i = 0; i < _comandosList.Count; i++)
{
if (GetDistance(_currentBot.Location, _comandosList.Values[i].Location) < (_currentBot.WasScaning ? _ScanRange : _VisibilityRange)
&&	!IsBotExistIn(_comandosList.Values[i],	_currentBot.Targets)	&&
_currentBot.ID != _comandosList.Values[i].ID)
{
_comandosList.RemoveAt(i); i--;
}
}

//	if(!_comandosList.ContainsKey(_ActivBotID) && _onlyMyCommand)
//	{
//	_ActivBotID = _comandosList.Values[0].ID;
//	}
}

private bool IsBotExistIn(CommandBot bot, List<Enemy> enemys)
{
foreach (Enemy en in enemys)
if	(en.Position.Equals(bot.Location)	|| en.Position.Equals(bot.PreviosLocation))
return true; return false;
}

private bool IsMyTank(Enemy en)
{
foreach (CommandBot bot in _comandosList.Values)
 

{
if	(bot.Location.Equals(en.Position)	||
bot.PreviosLocation.Equals(en.Position))
return true;
}

return false;
}

private int GetDistance(Position p1, Position p2)
{
int dx; int dy;
int distance;

dx = p1.X - p2.X;
dy = p1.Y - p2.Y;

distance = dx * dx + dy * dy; if (distance != 0)
distance = (int)Math.Sqrt(distance);

return distance;
}

private Position GetMinPath(List<Position> poss)
{
Position retPos = null; int dist = 0;
int mindist = GetDistance(new Position() { X = 0, Y = 0 }, new Position() { X
= _ArenaWidth, Y = _ArenaHeight });

if (poss.Count > 1)
{
foreach (Position pos in poss)
{
dist = GetDistance(_currentBot.Location, pos); if (dist < mindist)
{
mindist = dist; retPos = pos;
}
}
}
else
{
retPos = poss[0];
}

return retPos;
}

private Position GetMaxPath(List<Position> poss)
{
Position retPos = null; int dist = 0;
int maxdist = 0;

if (poss.Count > 1)
{
foreach (Position pos in poss)
{
dist = GetDistance(_currentBot.Location, pos); if (dist > maxdist)
{
maxdist = dist; retPos = pos;
}
}
}
else
{
retPos = poss[0];
}

return retPos;
}

private Enemy FindBestEnemyInAttrackRange()
 

{
Enemy retEnemy = null; if (_enemies.Count > 0)
{
int minHP = _MaxHP + 1; int dist = 0;

foreach (Enemy en in _enemies.Values)
{
 

_currentBot.Attackers))
 
if (IsEnemyExistIn(en, _currentBot.Targets) || IsEnemyExistIn(en,

{
dist = GetDistance(_currentBot.Location, en.Position); if (dist <= _AttrackRange && minHP > en.HitPoints)
{
 
minHP = en.HitPoints; retEnemy = en;
}
}
}
}

return retEnemy;
}

private Enemy FindBestEnemy()
{
Enemy retEnemy = null; if (_enemies.Count > 0)
{
int	minDist	=	GetDistance(_currentBot.Location,
_enemies.Values[0].Position);
retEnemy = _enemies.Values[0]; int dist = 0;

foreach (Enemy en in _enemies.Values)
{
dist = GetDistance(_currentBot.Location, en.Position); if (dist < minDist)
{
minDist = dist; retEnemy = en;
}
}
}
else if (_onlyMyCommand)
{
retEnemy = new Enemy(); retEnemy.Position = GetNearBotPosition();
}

return retEnemy;
}

private CommandBot GetNearBot()
{
Position p = GetNearBotPosition(); CommandBot retBot = null;

foreach (CommandBot bot in _comandosList.Values)
{
if (p.Equals(bot.Location)) retBot = bot;
}

return retBot;
}

private Position GetNearBotPosition()
{
List<Position> poss = new List<Position>();

foreach (CommandBot bot in _comandosList.Values)
{
if (bot.ID != _currentBot.ID) poss.Add(bot.Location);
}

return GetMinPath(poss);
 

}

private bool IsEnemyExistIn(Enemy enemy, List<Enemy> enemys)
{
foreach (Enemy en in enemys)
{
if (enemy.ID == en.ID) return true;
}

return false;
}

private Position GetRandPos()
{
Position pos = new Position(); Random rnd = new Random();
pos.X = rnd.Next(0, _ArenaWidth); pos.Y = rnd.Next(0, _ArenaHeight); return pos;
}
}
}





