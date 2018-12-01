<%@ page language="java" contentType="text/html; charset=utf-8"
    pageEncoding="utf-8"%>
<%
	String logined = (String)session.getAttribute("logined");
	String idcheck = (String)session.getAttribute("idcheck");
	String id = (String)session.getAttribute("id");
	String pw = (String)session.getAttribute("pw");
	String name = (String)session.getAttribute("name");
	String gender = (String)session.getAttribute("gender");
	String major = (String)session.getAttribute("major");
	String grade = (String)session.getAttribute("grade");
		
	if(id == null)
		id = "";
	if(pw == null)
		pw = "";
	if(name == null)
		name = "";
	if(gender == null)
		gender = "";
	if(major == null)
		major = "";
	if(grade == null)
		grade = "";
%>
<!DOCTYPE html>
<html>
	<head>
		<meta charset="utf-8">
		<meta name="viewport" content="width=device-width, initial-scale=1.0">
		<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css" integrity="sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO" crossorigin="anonymous">
		<script src="https://code.jquery.com/jquery-3.3.1.slim.min.js" integrity="sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo" crossorigin="anonymous"></script>
		<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.3/umd/popper.min.js" integrity="sha384-ZMP7rVo3mIykV+2+9J3UJ46jBk0WLaUAdn689aCwoqbBJiSnjAK/l8WvCWPIPm49" crossorigin="anonymous"></script>
		<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/js/bootstrap.min.js" integrity="sha384-ChfqqxuZUCnJSK3+MXmPNIyE6ZbWh2IMqE241rYiqJxyMiZ6OW/JmZQ5stwEULTy" crossorigin="anonymous"></script>
		<title>회원 가입</title>
		<script>
			function id_check(){
				var myform = document.form;
				myform.action = "IDcheck";
				myform.method = "POST";
				myform.submit();
			}
			function sign_up(){
				var id = document.getElementById("id").value;
				var pw = document.getElementById("pw").value;
				var pw2 = document.getElementById("pw2").value;
				var name = document.getElementById("name").value;
				var major = document.getElementById("major").value;
				
				if (id == ""){
					alert("아이디를 입력하세요");
					return;
				}
				
				if(pw != pw2){
					alert("비밀번호가 일치하지 않습니다.");
					return;
				}
				
				if(name == ""){
					alert("이름을 입력하세요.");
					return;
				}
				
				if(major == ""){
					alert("전공을 입력하세요.");
					return;
				}
				
				var myform = document.form;
				myform.action = "Signup";
				myform.method = "POST";
				myform.submit();
			}
		</script>
	</head>
	<body>
		<nav class="navbar navbar-expand-lg navbar-dark bg-info fixed-top">
			<a class="navbar-brand" href="#">임현호의 게시판</a>
			<button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
				<span class="navbar-toggler-icon"></span>
			</button>
			<div class="collapse navbar-collapse" id="navbarSupportedContent">
				<ul class="navbar-nav mr-auto">
					<li class="nav-item" style="text-align: right">
						<a class="nav-link" href="./">메인화면</a>
					</li>
					<li class="nav-item" style="text-align: right">
						<a class="nav-link" href="./Board.jsp">게시판</a>
					</li>
					<li class="nav-item" style="text-align: right">
						<a class="nav-link" href='javascript:void(0);' onclick="notyet()">추가 메뉴</a>
						<script>function notyet(){alert("아직 안만듬..");}</script>
					</li>
				</ul>
				<ul class="navbar-nav">
					<%
					// 로그인 되지 않은 경우 --> 로그인, 회원가입
					if (logined == null) {
						out.println(" <li class='nav-item' style='text-align: right'> ");
						out.println(" <a class='nav-link' href='./Login.jsp'>로그인</a> ");
						out.println(" </li> ");
						out.println(" <li class='nav-item' style='text-align: right'> ");
						out.println(" <a class='nav-link' href='./Signup.jsp'>회원가입</a> ");
						out.println(" </li> ");
					}
					// 로그인 된 경우 --> ID, 로그아웃
					else {
						out.println("  <script> ");
						out.println(" 	function logout(){ ");
						out.println(" 	 var form = document.createElement(\"form\"); ");
						out.println(" 	 document.body.appendChild(form); ");
						out.println(" 	 form.setAttribute(\"method\", \"POST\"); ");
						out.println("    form.setAttribute(\"action\", \"Logout\"); ");
						out.println("    form.submit(); ");
						out.println("   }; ");
						out.println("  </script> ");

						out.println(" <li class='nav-item' style='text-align: right'> ");
						out.println(" <a class='nav-link' href='./Info.jsp'>" + id + "</a> ");
						out.println(" </li> ");
						out.println(" <li class='nav-item' style='text-align: right'> ");
						out.println(" <a class='nav-link' href='javascript:void(0);' onclick='logout()'>로그아웃</a> ");
						out.println(" </li> ");
					}
					%>
				</ul>
			</div>
		</nav>
		<br><br><br>
		
		
		<div class="row">
			<div class="col-xl-4 col-md-3 col-1"></div>
			<div class="col-xl-4 col-md-6 col-10">
				<h3>회원 가입</h3>
				<hr>
				<form name="form">
		 		 	<div class="form-group">
					    <label>아이디</label>
					    <div class="form-inline">
							<input type="text" class="form-control" style="width: 70%" name="id" id="id" value="<%=id%>"
							<% if(idcheck != null) out.print(" readonly='readonly'"); %> >
							<input type="button" class="btn btn-info" style="width: 30%" value="<% if(idcheck == null) out.print("중복확인"); else out.print("확인완료"); %>" onclick="id_check()"
							<% if(idcheck != null) out.print(" disabled"); %> >
						</div>
					</div>
		 		 	<div class="form-group">
					    <label>비밀번호</label>
						<input type="password" class="form-control" name="pw" id="pw" value="<%=pw%>">
					</div>
		 		 	<div class="form-group">
					    <label>비밀번호 확인</label>
						<input type="password" class="form-control" name="pw2" id="pw2" value="<%=pw%>">
					</div>
		 		 	<div class="form-group">
					    <label>이름</label>
						<input type="text" class="form-control" name="name" id="name" value="<%=name%>">
					</div>
		 		 	<div class="form-group">
					    <label>성별</label>
					    <br>
						<div class="btn-group btn-group-toggle" style="width: 100%" data-toggle="buttons">
							<label class="btn btn-info <% if(!gender.equals("여"))out.print("active"); %>" style="width: 50%">
								<input type="radio" name="gender" value="남" autocomplete="off" <% if(!gender.equals("여"))out.print("checked"); %>>남
							</label>
							<label class="btn btn-info <% if(gender.equals("여"))out.print("active"); %>" style="width: 50%">
								<input type="radio" name="gender" value="여" autocomplete="off" <% if(gender.equals("여"))out.print("checked"); %>>여
							</label>
						</div>
					</div>
		 		 	<div class="form-group">
					    <label>전공</label>
						<input type="text" class="form-control" name="major" id="major" value="<%=major%>">
					</div>
		 		 	<div class="form-group">
					    <label>학년</label>
					    <br>
						<div class="btn-group btn-group-toggle" style="width: 100%" data-toggle="buttons">
							<label class="btn btn-info <% if(!grade.equals("2")&&!grade.equals("3")&&!grade.equals("4"))out.print("active"); %>" style="width: 25%">
								<input type="radio" name="grade" value="1" autocomplete="off" <% if(!grade.equals("2")&&!grade.equals("3")&&!grade.equals("4"))out.print("checked"); %>>1
							</label>
							<label class="btn btn-info <% if(grade.equals("2"))out.print("active"); %>" style="width: 25%">
								<input type="radio" name="grade" value="2" autocomplete="off" <% if(grade.equals("2"))out.print("checked"); %>>2
							</label>
							<label class="btn btn-info <% if(grade.equals("3"))out.print("active"); %>" style="width: 25%">
								<input type="radio" name="grade" value="3" autocomplete="off" <% if(grade.equals("3"))out.print("checked"); %>>3
							</label>
							<label class="btn btn-info <% if(grade.equals("4"))out.print("active"); %>" style="width: 25%">
								<input type="radio" name="grade" value="4" autocomplete="off" <% if(grade.equals("4"))out.print("checked"); %>>4
							</label>
						</div>
					</div>
					<br>
					<input type="button" class="btn btn-info" value="회원가입 완료" onclick="sign_up()">
				</form>	
			</div>
		</div>
		<br><br><br><br><br>
		
		
	</body>
</html>