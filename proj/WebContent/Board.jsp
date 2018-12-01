<%@ page language="java" contentType="text/html; charset=utf-8"
    pageEncoding="utf-8"%>
<%@ page import="javax.servlet.http.HttpSession" %>
<%@ page import="java.sql.*" %>
<%
String logined = (String)session.getAttribute("logined");
String pageNum = request.getParameter("pageNum");
String id = (String)session.getAttribute("id");
if(pageNum == null)
	pageNum = "1";
int page_int = Integer.parseInt(pageNum);
// 로그인 되지 않은 경우
if (logined == null) {
	session.invalidate();

	// 경고창 띄우고 첫 화면으로 이동
	out.println("<script>");
	out.println("alert('로그인을 하셔야 합니다.');");
	out.println("location.href = 'Login.jsp'"); 
	out.println("</script>"); 
}

// mssql 연결
Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
String connectionUrl = "jdbc:sqlserver://localhost:1433;Database=IP_final;IntegratedSecurity=true";
Connection conn = DriverManager.getConnection(connectionUrl);

// Query 진행
PreparedStatement pstmt1 = conn.prepareStatement("SELECT COUNT (*) AS cnt FROM article_data");
ResultSet rs = pstmt1.executeQuery();
rs.next();

// 총 게시글의 수
int cnt = Integer.parseInt(rs.getString("cnt"));
int max_page = (cnt + 9) / 10;
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
		<title>게시판</title>
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
		<div class="container">
			<h4>게시판</h4>
			<hr>

			<table class="table table-bordered table-hover">
				<thead>
					<tr>
						<th style="width: 10%">번호</th>
						<th style="width: 40%">제목</th>
						<th style="width: 15%">작성자</th>
						<th style="width: 15%">작성시간</th>
					</tr>
				</thead>
				<tbody>
					<%
					if (logined != null) {
						
						// 10개씩 뽑아내기
						String offset_pageNum = Integer.toString((Integer.parseInt(pageNum) - 1) * 10);
						String query1 = "SELECT A.a_num, A.title, A.time, A.u_num, U.id" + 
						" FROM article_data as A join user_data as U on A.u_num = U.u_num ORDER BY A.a_num DESC OFFSET "
						+ offset_pageNum + " ROWS FETCH NEXT 10 ROWS ONLY";
						
						// Query 진행
						PreparedStatement pstmt2 = conn.prepareStatement(query1);
						rs = pstmt2.executeQuery();
						
						while(rs.next()){
							// 밀리 초 삭제
							String time = rs.getString("time");
							time = time.substring(0, time.lastIndexOf("."));
							
							out.println(" <tr> ");
							out.println("  <td>" + rs.getInt("a_num") + "</td>  ");
							out.println("  <td>" + rs.getString("title") + "</td> ");
							out.println("  <td>" + rs.getString("id") + "</td> ");
							out.println("  <td>" + time + "</td> ");
							out.println(" </tr> ");
						}
						
						rs.close();
						pstmt2.close();
						pstmt1.close();
						conn.close();
					}
					%>
				</tbody>
			</table>
			<div class="container">
				<div class="row justify-content-end">
					<input type="button" class="btn btn-info" value="글쓰기" onclick="location='NewArticle.jsp'")>
				</div>
			</div>
			<nav aria-label="Page navigation example">
				<ul class="pagination justify-content-center">
					<li class="page-item <% if((page_int - 1) / 5 == 0) out.print("disabled"); %>">
						<a class="page-link" href="#">&laquo;</a>
					</li>
					<%
						int TOTAL_PAGE = 5;
						for(int i = 1, j = (page_int - 1) / 5; i <= TOTAL_PAGE; i++){
							out.print(" <li class='page-item ");
							if(TOTAL_PAGE * j + i == page_int)
								out.print("active");
							out.print("'><a class='page-link' href='javascript:void(0);' onclick='pagemove(");
							out.print(TOTAL_PAGE * j + i);
							out.print(")'>");
							out.print(TOTAL_PAGE * j + i);
							out.println("</a></li> ");
							if(TOTAL_PAGE * j + i == max_page)
								break;
						}
						out.println("<script>");
						out.println(" function pagemove(next_page) {");
						out.println("  var cur_page = " + page_int);
						out.println("  if (cur_page != next_page)");
						out.println("   location='Board.jsp?pageNum=' + next_page");
						out.println(" }");
						out.println("</script>");
					%>
					<li class="page-item <% if((page_int - 1) / 5 == (max_page - 1) / 5) out.print("disabled"); %>">
						<a class="page-link" href="#">&raquo;</a>
      				</li>
				</ul>
			</nav>
		</div>
	</body>
</html>