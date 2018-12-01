package com.servlet;

import java.sql.*;

import java.io.IOException;
import java.io.PrintWriter;
import java.io.UnsupportedEncodingException;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

/**
 * Servlet implementation class IDcheck
 */
@WebServlet("/IDcheck")
public class IDcheck extends HttpServlet {
	private static final long serialVersionUID = 1L;
       
    /**
     * @see HttpServlet#HttpServlet()
     */
    public IDcheck() {
        super();
    }

	/**
	 * @see HttpServlet#doPost(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
		request.setCharacterEncoding("UTF-8");
		response.setCharacterEncoding("UTF-8");
		response.setContentType("text/html; charset=UTF-8");
		
		id_check(request, response);
	}

	protected void id_check(HttpServletRequest request, HttpServletResponse response) {
		try {
			// mssql 연결
			Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
			String connectionUrl = "jdbc:sqlserver://localhost:1433;Database=IP_final;IntegratedSecurity=true";
			Connection conn = DriverManager.getConnection(connectionUrl);
			
			// 한글 인코딩
			request.setCharacterEncoding("UTF-8");
			
			// 입력한 것들 받아오기
			String id = request.getParameter("id");
			String pw = request.getParameter("pw");
			String name = request.getParameter("name");
			String gender = request.getParameter("gender");
			String major = request.getParameter("major");
			String grade = request.getParameter("grade");

			// Query 진행
			PreparedStatement pstmt = conn.prepareStatement("SELECT id FROM user_data WHERE id IN( ? )");
			pstmt.setString(1, id);
			ResultSet rs = pstmt.executeQuery();

			// 넘겨줄 Attribute
			HttpSession session = request.getSession();
			session.setAttribute("id", id);
			session.setAttribute("pw", pw);
			session.setAttribute("name", name);
			session.setAttribute("gender", gender);
			session.setAttribute("major", major);
			session.setAttribute("grade", grade);

			// 알림창 띄우는 용도
			response.setContentType("text/html; charset=UTF-8");
			PrintWriter out = response.getWriter();
			
			// DB에 해당 ID가 존재하는 경우
			if (rs.next()) {
				// 창 띄워준 후 회원가입 화면으로 이동
				out.println("<script>");
				out.println("alert('중복된 ID 입니다.');");
				out.println("location.href = 'Signup.jsp'"); 
				out.println("</script>"); 
				out.close();
			}
			// 사용 가능한 ID인 경우
			else {
				session.setAttribute("idcheck", "check_complete");
				
				// 창 띄워준 후 회원가입 화면으로 이동
				out.println("<script>");
				out.println("alert('사용가능한 ID 입니다.');");
				out.println("location.href = 'Signup.jsp'"); 
				out.println("</script>"); 
				out.close();
			}

			rs.close();
			pstmt.close();
			conn.close();
		} catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		} catch (ClassNotFoundException e) {
			e.printStackTrace();
		} catch (SQLException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
}
