# [TASK008] - Scene System Organization for Minipoll V5

**Status:** In Progress  
**Added:** July 8, 2025  
**Updated:** July 8, 2025

## Original Request
לארגן את המערכות הרבות במשחק לסצנות נפרדות כדי לקבל שליטה טובה יותר על הפרויקט הגדול. לא ליצור סצנות מיותרות, ולשלב מערכות שיכולות לעבוד יחד.

## Thought Process

אחרי בדיקת המערכות הקיימות, המשחק מכיל המון מערכות מורכבות:

### Core Systems (מערכות ליבה):
- MinipollCore - המערכת המרכזית
- MinipollBrain - מנוע ה-AI
- MinipollHealth & MinipollStats - מערכות בסיס
- MinipollNeedsSystem - מערכת צרכים
- MinipollEmotionsSystem - מערכת רגשות
- MinipollMemorySystem - מערכת זיכרון

### Social Systems (מערכות חברתיות):
- MinipollSocialRelations - יחסים חברתיים
- MinipollTribeSystem - מערכת שבטים
- MinipollBattleSystem - מערכת קרבות

### World Systems (מערכות עולם):
- MinipollBuildingSystem - מערכת בנייה
- MinipollEconomySystem - מערכת כלכלה
- FoodChainManager - שרשרת מזון
- WorldManager - ניהול עולם

### Advanced Systems (מערכות מתקדמות):
- MinipollArtifactSystem - מערכת חפצים
- MinipollNarrativeSystem - מערכת סיפור
- MinipollGeneticsSystem - מערכת גנטיקה
- MinipollReproductionSystem - מערכת רבייה

### Controllers (בקרים):
- MinipollMovementController - תנועה
- MinipollVisualController - ויזואליה
- MinipollBlinkController - מצמוץ
- MinipollWorldInteraction - אינטראקציה עם העולם

## Implementation Plan

### Scene 1: CoreCreatureScene
**מטרה**: פיתוח וטסטינג של המערכות הבסיסיות של היצור
**מערכות**:
- MinipollCore + MinipollBrain 
- MinipollHealth + MinipollStats
- MinipollNeedsSystem + MinipollEmotionsSystem
- MinipollMemorySystem
- MinipollMovementController + MinipollVisualController + MinipollBlinkController
- סביבה פשוטה עם אוכל, מים, מקומות מנוחה

### Scene 2: SocialSystemsScene  
**מטרה**: פיתוח מערכות חברתיות ויחסים בין יצורים
**מערכות**:
- כל מה מ-CoreCreatureScene
- MinipollSocialRelations + MinipollTribeSystem
- MinipollBattleSystem
- מספר יצורים לאינטראקציה

### Scene 3: WorldEconomyScene
**מטרה**: מערכות עולם, כלכלה ובנייה
**מערכות**:
- MinipollBuildingSystem + MinipollEconomySystem
- FoodChainManager
- WorldManager + MinipollWorldInteraction
- UtilityAI for complex decisions
- סביבה עשירה עם משאבים

### Scene 4: AdvancedFeaturesScene
**מטרה**: מערכות מתקדמות ותכונות מיוחדות
**מערכות**:
- MinipollGeneticsSystem + MinipollReproductionSystem
- MinipollArtifactSystem + MinipollNarrativeSystem
- MinipollTaskSystem + MinipollMountSystem
- מערכות ML-Agents integration

### Scene 5: IntegrationTestScene
**מטרה**: טסטינג של כל המערכות יחד
**מערכות**:
- כל המערכות פועלות יחד
- Performance testing
- Full gameplay experience

## Progress Tracking

**Overall Status:** In Progress - 25%

### Subtasks
| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 8.1 | Analyze existing systems | Complete | 2025-07-08 | Analyzed all scripts and systems |
| 8.2 | Create CoreCreatureScene | Complete | 2025-07-08 | Scene created with basic environment and navigation |
| 8.3 | Setup creature prefab with core components | In Progress | 2025-07-08 | Working on MinipollCore integration |
| 8.4 | Create SocialSystemsScene | Not Started | - | Multi-creature interactions |
| 8.5 | Create WorldEconomyScene | Not Started | - | Economy and building systems |
| 8.6 | Create AdvancedFeaturesScene | Not Started | - | Advanced systems testing |
| 8.7 | Create IntegrationTestScene | Not Started | - | Full integration testing |
| 8.8 | Setup scene navigation system | Not Started | - | Easy switching between scenes |

## Progress Log
### 2025-07-08
- Created task and analyzed existing project structure
- Identified 30+ different systems that need organization
- Planned 5-scene structure for systematic development
- **COMPLETED**: Created all 5 scenes with basic environment setup
- **COMPLETED**: Implemented SceneNavigationManager with Tab navigation and 1-5 key shortcuts
- **COMPLETED**: Set up CoreCreatureScene with ground, lighting, food/water sources, rest area
- **COMPLETED**: Created SimpleCreatureController for testing basic functionality (3 test creatures working)
- **IN PROGRESS**: Starting integration of real MinipollCore systems
- Next: Fix compilation issues with MinipollCore and integrate real systems
