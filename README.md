BasicApp
========

스토어에 앱을 몇번 올려보니 정작 앱 기능 구현이 어느 정도 되도 스토어 인증 기준에 맞추기 위해 마지막에 꼭 해야 하는 귀찮은 작업들이 여럿 있더라고요.(스냅뷰 추가, 개인보호 정책 추가 등) 그래서 기본 템플릿 앱에 해당 부분들을 추가해서 새로운 템플릿을 만들어 봤습니다. 오픈소스로 업데이트를 진행할 계획이라 필요한 기능 및 버그가 있으면 얼마든지 의견 및 Pull Request 보내주세요.

구현된 기능
- Add Basic References - Microsoft Ad SDK(http://go.microsoft.com/?linkid=9815330), Callisto Extenstion(http://visualstudiogallery.msdn.microsoft.com/0526563b-7a48-4b17-a087-a35cea701052) 설치 및 레퍼런스 등록 필요
- Privacy Policy Link - App.xaml.cs에서 실제 웹페이지로 URL 수정 필요
- Microsoft Advertising AdControl - http://pubcenter.microsoft.com 에서 계정 등록, MainPage.xaml에서 ApplicationId 및 AdUnitId 수정 필요
- Trial Version 표시 및 구매 - App.xaml.cs에서 CurrentAppProxy 변경 필요
- Tile Images
- Snapped View
- Language Resources
- About Page
- Options Page

고려 중인 업데이트
- 공유 및 검색 참 기본 기능
- 선택 가능한 타일 이미지 세트 추가
- 앱바 버튼 스타일 리소스
- 3G 데이터망 사용 옵션(비디오,오디오 스트리밍 앱에 필요)
- 라이브 타일 기본 기능
- 페이스북 등 소셜 SNS 로그인 및 공유 기능
- JavaScript 버전 템플릿
