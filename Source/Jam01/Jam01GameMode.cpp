// Copyright 1998-2017 Epic Games, Inc. All Rights Reserved.

#include "Jam01GameMode.h"
#include "Jam01HUD.h"
#include "Jam01Character.h"
#include "UObject/ConstructorHelpers.h"

AJam01GameMode::AJam01GameMode()
	: Super()
{
	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnClassFinder(TEXT("/Game/FirstPersonCPP/Blueprints/FirstPersonCharacter"));
	DefaultPawnClass = PlayerPawnClassFinder.Class;

	// use our custom HUD class
	HUDClass = AJam01HUD::StaticClass();
}
