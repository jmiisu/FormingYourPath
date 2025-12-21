using UnityEngine;

public enum MAP_STATE
{
    STAGE_BLOCK,    // 스테이지 고정 블록 (x)
    EXIT,           // 출구
    BASIC,          // 기본 블록 (x)
    STAIR,          // 계단 블록 
    PLAYER_POS,     // 플레이어 위치
    EMPTY,          // 빈 곳
};

/*
 * 0 0 0 0 0 0 
 * 0 0 0 0 0 0 
 * 0 0 0 0 0 0
 * 1 0 2 P 3 2
 * 1 1 1 1 1 1
 */