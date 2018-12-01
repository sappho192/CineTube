<%@ page language="java" contentType="text/html; charset=utf-8"
    pageEncoding="utf-8"%>
<%@ page import="javax.servlet.http.HttpSession" %>
<%
	session.invalidate();
	String logined = null;
	String id = null;
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
		<title>로그인</title>
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
				<h3>로그인</h3>
				<hr>
				<form name="form" method="POST" action="Login">
		 		 	<div class="form-group">
					    <label>아이디</label>
						<input type="text" class="form-control" name="id">
					</div>
		 		 	<div class="form-group">
					    <label>비밀번호</label>
						<input type="password" class="form-control" name="pw">
					</div>
					<input type="submit" class="btn btn-info" value="로그인" onclick="login()">
					<input type="button" class="btn btn-info" value="회원가입" onclick="location='Signup.jsp'">
				</form>	
			</div>
		</div>
		<br><br><br><br><br>
		
		
	</body>
</html>